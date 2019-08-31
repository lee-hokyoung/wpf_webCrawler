using CefSharp;
using CefSharp.Wpf;
using HtmlAgilityPack;
using MySql.Data.MySqlClient;
using PuppeteerSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace webCrawler
{

    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public ChromiumWebBrowser browser;
        public string strConn = Properties.Settings.Default.strConn;
        public string strHtml = "", detailHtml = "";
        public List<string> html_node = null;
        public List<string> success_ids = null;
        public List<Tuple<string, string>> error_ids = null;
        public string main_url = "https://world.taobao.com/";
        public string login_url = "https://world.taobao.com/markets/all/login";
        public string login_frame_url = "https://login.taobao.com/member/login.jhtml?style=miniall&newMini2=true&full_redirect=true&&redirectURL=http%3A%2F%2Fworld.taobao.com%2F&from=worldlogin&minipara=1,1,1&from=worldlogin";
        public PuppeteerSharp.Page puppeteer_page = null;
        public Browser pup_browser;
        // 상품리스트 파싱하기 & prd_list 에 저장하기
        ArrayList prd_list = new ArrayList();           // Product 클래스를 담아두는 ArrayList
        List<string> id_list = new List<string>();      // 상품 중복 파싱을 방지하기 위한 상품코드 저장하는 List
        List<ViewModel.ProductViewModel> prdView_list = new List<ViewModel.ProductViewModel>();
        List<ViewModel.ResultViewModel> result_view_list = new List<ViewModel.ResultViewModel>();
        ViewModel.CategoryViewModel selectedCategory = null;
        int idx = 1;

        // 제외상품 리스트 관련 변수
        List<ViewModel.delProductViewModel> delPrdView_list = new List<ViewModel.delProductViewModel>();

        CefSettings settings = new CefSettings();

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new ViewModel.ProductViewModel();
            InitializeChromium();
            getDbData();
            cbCategory.ItemsSource = webCrawler.Contoller.Category.readCategory("A");
            InitTaobao();
        }
        // puppeteer 로그인
        private async void InitTaobao()
        {
            try
            {
                string test_url = "https://intoli.com/blog/not-possible-to-block-chrome-headless/chrome-headless-test.html";
                string test_url2 = "https://bot.sannysoft.com/";
                string chrome_exe_path = "C:\\Program Files (x86)\\Google\\Chrome\\Application\\chrome.exe";
                doc_opacity.Visibility = Visibility.Visible;
                doc_status.Visibility = Visibility.Visible;
                txt_crawling_count.Text = "프로그램 작동에 필요한 리소스를 수집하는 중입니다.";
                txt_crawling_status.Text = "크롬브라우저 구동중" + System.Environment.NewLine +  "이 창이 닫히면 구동된 크롬 브라우저에서 로그인해주세요";
                await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultRevision);
                string[] args = new string[] {
                    "--no-sandbox",
                    "--disable-setuid-sandbox",
                    "--disable-infobars",
                    "--window-position=0,0",
                    "--ignore-certifcate-errors",
                    "--ignore-certifcate-errors-spki-list",
                    "--disable-webdriver"
                };
                pup_browser = await Puppeteer.LaunchAsync(new LaunchOptions
                {
                    Headless = false,
                    Args = args,
                    IgnoreHTTPSErrors = true,
                    ExecutablePath = chrome_exe_path,
                    Timeout = 3000
                });
                Dictionary<string, string> header = new Dictionary<string, string>();
                header.Add("Referer", "https://world.taobao.com/");
                var overrideNavigatorWebdriver = @"() => {
                    Object.defineProperty(navigator, 'webdriver', {
                        get: () => false,
                    });
                }";
                var overridePermission = @"() => {
                    const originalQuery = window.navigator.permissions.query;
                    return window.navigator.permissions.query = (parameters) => (
                        parameters.name === 'notifications' ?
                        Promise.resolve({ state: Notification.permission }) :
                        originalQuery(parameters)
                    );
                }";
                var overrideChrome = @"() => {
                    Object.defineProperty(navigator, 'chrome', {
                        runtime: {},
                    });
                }";
                var overridePlugin = @"() => {
                    Object.defineProperty(navigator, 'plugins', {
                        get: () => [1, 2, 3, 4, 5],
                    });
                }";
                puppeteer_page = await pup_browser.NewPageAsync();
                await puppeteer_page.SetUserAgentAsync("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/75.0.3770.142 Safari/537.36");
                await puppeteer_page.SetExtraHttpHeadersAsync(header);
                await puppeteer_page.EvaluateFunctionOnNewDocumentAsync(overrideNavigatorWebdriver);
                await puppeteer_page.EvaluateFunctionOnNewDocumentAsync(overridePermission);
                await puppeteer_page.EvaluateFunctionOnNewDocumentAsync(overrideChrome);
                await puppeteer_page.EvaluateFunctionOnNewDocumentAsync(overridePlugin);
                await puppeteer_page.SetViewportAsync(new ViewPortOptions { Width = 0, Height = 0 });
                await puppeteer_page.GoToAsync(login_frame_url, new NavigationOptions
                {
                     WaitUntil = new WaitUntilNavigation[]
                     {
                        WaitUntilNavigation.Load
                     }
                });
                //await puppeteer_page.ScreenshotAsync("d:\\screenshot.png");
                //await puppeteer_page.TypeAsync("#TPL_username_1", "supereggsong");
                //await puppeteer_page.TypeAsync("#TPL_password_1", "alsdud1218!");
                //var slider = await puppeteer_page.QuerySelectorAsync("#nc_1_wrapper");
                //if (slider != null)
                //{
                //    await puppeteer_page.Mouse.MoveAsync(260, 210);
                //    await puppeteer_page.Mouse.DownAsync();
                //    await puppeteer_page.Mouse.MoveAsync(550, 210);
                //    await puppeteer_page.Mouse.UpAsync();
                //}
                //await puppeteer_page.ClickAsync("#J_SubmitStatic");
                //await puppeteer_page.WaitForTimeoutAsync(5000);
                //var current_url = puppeteer_page.Url;
                //if(current_url.IndexOf("login") > -1)
                //{
                //    await puppeteer_page.TypeAsync("#TPL_username_1", "supereggsong");
                //    await puppeteer_page.TypeAsync("#TPL_password_1", "alsdud1218!");
                //    if (slider != null)
                //    {
                //        await puppeteer_page.Mouse.MoveAsync(260, 210);
                //        await puppeteer_page.Mouse.DownAsync();
                //        await puppeteer_page.Mouse.MoveAsync(550, 210);
                //        await puppeteer_page.Mouse.UpAsync();
                //    }
                //}
            }
            catch(TimeoutException tex)
            {
                MessageBox.Show(tex.ToString());
                throw tex;
            }
            catch(Exception e)
            {
                MessageBox.Show(e.ToString());
                throw e;
            }
            finally
            {
                doc_opacity.Visibility = Visibility.Collapsed;
                doc_status.Visibility = Visibility.Collapsed;
                txt_crawling_count.Text = "";
            }
            
        }
        private void InitializeChromium()
        {
            main_doc.Visibility = Visibility.Visible;
            myDb_doc.Visibility = Visibility.Collapsed;
            //CefSettings settings = new CefSettings()
            //{
            //    CachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CefSharp\\Cache")
            //};
            settings.CachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CefSharp\\Cache");
            Cef.Initialize(settings);

            browser = new ChromiumWebBrowser();
            browser.Address = main_url;
            
            Grid.SetRow(browser, 0);
            
            grid.Children.Add(browser);

            browser.FrameLoadEnd += WebZBrowserFrameLoadEnded;
            browser.FrameLoadStart += WebZBrowserFrameLoadStarted;
            browser.LoadingStateChanged += OnLoadingStateChanged;
        }

        private void WebZBrowserFrameLoadStarted(object sender, FrameLoadStartEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() => txtStatus.Text = "Loading..."));
        }

        private void OnLoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
        {
            if (!e.IsLoading)
            {
                browser.GetSourceAsync().ContinueWith(task =>
                {
                    strHtml = task.Result;
                    //if (strHtml.IndexOf("描述加载中") == -1 && html_node != null)
                    if (strHtml.IndexOf("tstart") > -1 && html_node != null)
                    {
                        detailHtml = task.Result;
                        //browser.ViewSource();
                        html_node.Add(detailHtml);
                    }
                });
            }
        }
        private void WebZBrowserFrameLoadEnded(object sender, FrameLoadEndEventArgs e)
        {
            if (e.Frame.IsMain)
            {
                //new Task(() =>
                //{
                //    // Wait a little bit, because Chrome won't have rendered the new page yet.
                //    // There's no event that tells us when a page has been fully rendered.
                //    Thread.Sleep(100);

                //    // Wait for the screenshot to be taken.
                //    var task = browser.ScreenshotAsync();
                //    task.Wait();

                //    // Make a file to save it to (e.g. C:\Users\jan\Desktop\CefSharp screenshot.png)
                //    var screenshotPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "CefSharp screenshot.png");

                //    Console.WriteLine();
                //    Console.WriteLine("Screenshot ready.  Saving to {0}", screenshotPath);

                //    // Save the Bitmap to the path.
                //    // The image type is auto-detected via the ".png" extension.
                //    task.Result.Save(screenshotPath);

                //    // We no longer need the Bitmap.
                //    // Dispose it to avoid keeping the memory alive.  Especially important in 32-bit applications.
                //    task.Result.Dispose();

                //    Console.WriteLine("Screenshot saved.  Launching your default image viewer...");

                //    // Tell Windows to launch the saved image.
                //    Process.Start(screenshotPath);

                //    Console.WriteLine("Image viewer launched.  Press any key to exit.");
                //}).Start();


                //if (e.Url == main_url) {
                //    var c = Cef.GetGlobalCookieManager();
                //}
                //browser.GetSourceAsync().ContinueWith(task =>
                //{
                //    strHtml = task.Result;
                //    browser.ViewSource();
                //    if(html_node != null) html_node.Add(strHtml);
                //});
            }
            Dispatcher.BeginInvoke((Action)(() => txtStatus.Text = "Loading Complete !!"));
        }

        private void getData(string source)
        {
            // MVVM DataGridViewModel
            //DataGridViewModel 
            try
            {
                // 카테고리 구하기. 현재 URL의 파라미터 중 q 값을 가져옴. URL DECODER가 필요함
                if (browser.Address.Split('?').Length < 2) return;
                string[] cate_arr = browser.Address.Split('?')[1].Split('&');
                string category = "";
                foreach(string name in cate_arr)
                {
                    if (name.Split('=')[0] == "q") category = WebUtility.UrlDecode(name.Split('=')[1]);
                }

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(source);
                HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//div[@data-category='auctions']");    // 1. 각각의 상품을 nodes 에 담기
                if (nodes == null) return;

                BitmapImage bi = new BitmapImage();
                HtmlDocument item = new HtmlDocument();
                string nid, src, prd_name, detailYn, isExist, prd_status;
                foreach (var node in nodes)
                {
                    item = new HtmlDocument();
                    item.LoadHtml(node.InnerHtml);
                    HtmlNodeCollection pic = item.DocumentNode.SelectNodes("//a[contains(@class, 'J_ItemPicA')]");  // 2-1. nodes에 있는 상품의 a 태그
                    HtmlNodeCollection img = item.DocumentNode.SelectNodes("//img[contains(@class, 'J_ItemPic')]"); // 2-2. nodes에 있는 상품의 img 태그

                    nid = pic[0].Attributes["data-nid"].Value;  // taobao 상품코드
                    if (id_list.IndexOf(nid) == -1)
                    {
                        // 기존에 저장되어 있는 DB 값에 id 값이 있으면 수집된 정보라는 것을 표시
                        var query = (from Prd_Store prd in db_list
                                     where prd.Id == nid
                                     select prd).SingleOrDefault();
                        // 상품상태 확인
                        if (query != null) {
                            if(query.Prd_status == "9") continue;
                            prd_status = query.Prd_status;
                        } 
                        else prd_status = "";

                        src = img[0].Attributes["data-src"].Value;
                        prd_name = img[0].Attributes["alt"].Value;

                        id_list.Add(nid);
                        bi = new BitmapImage();
                        bi.BeginInit();
                        bi.UriSource = new Uri("http:" + src, UriKind.RelativeOrAbsolute);
                        bi.EndInit();
                        if (query == null)
                        {
                            detailYn = "X";
                            isExist = "X";
                        }
                        else
                        {
                            if (query.Detail_yn == "1") detailYn = "O";
                            else detailYn = "X";
                            isExist = "O";
                        }
                        prdView_list.Add(new ViewModel.ProductViewModel(
                            false,      // 체크박스
                            nid,        // 상품코드
                            bi,         // 상품이미지
                            idx++,      // 순서
                            detailYn,   // 상세정부수집 여부 : 'O, X'
                            isExist,    // 상품등록여부 : 'O, X'
                            "",         // 등록타입 : '[New], [Updated]'
                            prd_name,         // 상품명
                            category,    // 카테고리
                            prd_status  // 상품상태
                            )
                        );
                    }
                }
                dgTable.ItemsSource = null;
                dgTable.ItemsSource = prdView_list;
                btnStoreDB.IsEnabled = true;
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
                throw ex;
            }
        }
        // 상품등록 여부 확인
        // 1. 해당 아이디의 상품정보를 읽어 온다. category 별로 나누는 것도 고려해 볼 것. 우선은 전체 상품 읽어 오기
        // 2. 불러온 상품 정보는 USER_PRD_LIST 에 담아둔다
        // TODO USER_PRD_LIST 만들것
        ArrayList db_list = null;
        List<string> p_id_list = new List<string>();
        // DB에 저장되어 있는 리스트 불러오기 -> db_list에 저장(Prd_Store 클래스)
        
        // 상품정보 파싱
        private void parsingPrdDetail(List<string> html_node)
        {
            HtmlDocument doc = null, doc_img = null;
            HtmlNodeCollection node_id = null, img_wrap = null, imgs = null, price = null, promo = null, opts = null, stock = null, attr = null, additional_image = null, opts_with_img = null, prd_brand = null;
            string[] strImg = null;

            StringBuilder sUPDATE = new StringBuilder("INSERT INTO tmp(id, prd_price, prd_promo, prd_stock, prd_opt_imgs, detail_img, prd_brand, " +
                "opt_1, opt_val_1, opt_2, opt_val_2, opt_3, opt_val_3, prd_attr, add_img_1, add_img_2, add_img_3, add_img_4, updated_date) VALUES ");
            string sql_id = "", sql_prd_price = "", sql_prd_promo = "", sql_prd_stock = "", sql_detail_img = "", sql_prd_brand = "",
                sql_opt_1 = "", sql_opt_val_1 = "", sql_opt_2 = "", sql_opt_val_2 = "", sql_opt_3 = "", sql_opt_val_3 = "", sql_prd_attr = "",
                sql_add_img_1 = "", sql_add_img_2 = "", sql_add_img_3 = "", sql_add_img_4 = "", sql_opt_imgs = "";
            List<string> update_rows = new List<string>();
            List<string> failed_codes = new List<string>();
            List<string> opt_imgs = new List<string>();
            MySqlConnection conn = null;

            try
            {
                txt_crawling_status.Text = "DB 저장 중";
                conn = new MySqlConnection(strConn);

                foreach (var node in html_node)
                {
                    sql_id = ""; sql_prd_price = ""; sql_prd_promo = ""; sql_prd_stock = ""; sql_detail_img = ""; sql_prd_brand = "";
                    sql_opt_1 = ""; sql_opt_val_1 = ""; sql_opt_2 = ""; sql_opt_val_2 = ""; sql_opt_3 = ""; sql_opt_val_3 = ""; sql_prd_attr = "";
                    sql_add_img_1 = ""; sql_add_img_2 = ""; sql_add_img_3 = ""; sql_add_img_4 = ""; sql_opt_imgs = "";
                    try
                    {
                        if (node == null)
                            continue;
                        // 상품 속성 : prd_attr
                        doc = new HtmlDocument();
                        doc.LoadHtml(node);

                        node_id = doc.DocumentNode.SelectNodes("//div[@id='LineZing']");
                        if (node_id != null)
                        {
                            sql_id = node_id[0].Attributes["itemid"].Value;
                        }
                        else
                        {
                            node_id = doc.DocumentNode.SelectNodes("//div[@id='J_Pine']");
                            if (node_id != null) sql_id = node_id[0].Attributes["data-itemid"].Value;
                        }
                        // 상품 상세 이미지 : prd_img
                        img_wrap = doc.DocumentNode.SelectNodes("//div[contains(@class, 'ke-post')]");
                        if (img_wrap != null)
                        {
                            //if(img_wrap[0].ChildNodes["div"] != null)
                            //{

                            //}else if(img_wrap[0].ChildNodes["p"] != null)
                            //{

                            //}

                            doc_img = new HtmlDocument();
                            doc_img.LoadHtml(img_wrap[0].InnerHtml);
                            var p_tags = doc_img.DocumentNode.SelectNodes("//p/img");
                            if(p_tags != null)
                            {
                                var strP_tags = new string[p_tags.Count];
                                for (var i = 0; i < p_tags.Count; i++)
                                {
                                    if (p_tags[i].Attributes["data-ks-lazyload"] != null)
                                    {
                                        strP_tags[i] = string.Format("<img src='{0}'>", p_tags[i].Attributes["data-ks-lazyload"].Value);
                                    }
                                    else if (p_tags[i].Attributes["img-ks-lazyload"] != null)
                                    {
                                        strP_tags[i] = string.Format("<img src='{0}'>", p_tags[i].Attributes["data-ks-lazyload"].Value);
                                    }
                                    
                                }
                                sql_detail_img += String.Join("", strP_tags);
                            }
                            var div_p_font_img = doc_img.DocumentNode.SelectNodes("//div/p/font/img");
                            if(div_p_font_img != null)
                            {
                                var strDiv_p_font_img = new string[div_p_font_img.Count];
                                for (var i = 0; i < div_p_font_img.Count; i++)
                                {
                                    strDiv_p_font_img[i] = string.Format("<img src='{0}'>", div_p_font_img[i].Attributes["data-ks-lazyload"].Value);
                                }
                                sql_detail_img += String.Join("", strDiv_p_font_img);
                            }
                            else
                            {
                                imgs = doc_img.DocumentNode.SelectNodes("//img[@data-ks-lazyload]");
                                if (imgs != null)
                                {
                                    strImg = new string[imgs.Count];
                                    for (var i = 0; i < imgs.Count; i++)
                                    {
                                        strImg[i] = string.Format("<img src='{0}'>", imgs[i].Attributes["data-ks-lazyload"].Value);
                                    }
                                    //sql_detail_img = String.Join("&$%", strImg);
                                    sql_detail_img += String.Join("", strImg);
                                }
                            }
                        }
                        // 타오바오 상품가격 : prd_price
                        price = doc.DocumentNode.SelectNodes("//dl[contains(@class,'tm-price-panel')]/dd/span");
                        if (price != null) sql_prd_price = price[0].InnerText;
                        else
                        {
                            price = doc.DocumentNode.SelectNodes("//strong[@id='J_StrPrice']");
                            if (price != null) sql_prd_price = price[0].InnerText;
                        }

                        // 프로모션 가격(할인된 가격, 판매가격) : prd_promo
                        promo = doc.DocumentNode.SelectNodes("//div[@class='tm-promo-price']/span");
                        if (promo != null) sql_prd_promo = promo[0].InnerText;
                        else
                        {
                            promo = doc.DocumentNode.SelectNodes("//em[@id='J_PromoPriceNum']");
                            if (promo != null) sql_prd_promo = promo[0].InnerText;
                        }
                        // 상품 옵션
                        opts = doc.DocumentNode.SelectNodes("//dl[contains(@class, 'tm-sale-prop')]");
                        if (opts == null)
                        {
                            opts = doc.DocumentNode.SelectNodes("//dl[contains(@class, 'J_Prop')]");
                        }
                        if (opts != null)
                        {
                            List<string> str_opts_value = new List<string>(); // 옵션 값을 담아두는 변수 선언
                            string opt_val = "";    // 옵션명을 담아두는 변수
                            for (var i = 0; i < opts.Count; i++)
                            {
                                string opt_name = opts[i].ChildNodes["dt"].InnerHtml;
                                HtmlNodeCollection opt_vals = opts[i].ChildNodes["dd"].ChildNodes["ul"].SelectNodes("li");
                                str_opts_value = new List<string>();

                                // 옵션에 재고가 없는 경우는 가져오지 않음
                                bool isSoldOut = false;
                                foreach (HtmlNode child in opt_vals)
                                {
                                    isSoldOut = false;
                                    if(child.Attributes["class"] != null)
                                    {
                                        // class에 tb-out-of-stock 이라는 글이 있을 경우 수집 제외.
                                        if(child.Attributes["class"].Value == "tb-out-of-stock")
                                        {
                                            isSoldOut = true;
                                        }
                                    }
                                    if(isSoldOut == false) str_opts_value.Add(child.ChildNodes["a"].ChildNodes["span"].InnerText);
                                }
                                opt_val = string.Join(",", str_opts_value); //  옵션값 -> 콤마(,)로 구분함.
                                switch (i)
                                {
                                    case 0:
                                        sql_opt_1 = opt_name;
                                        sql_opt_val_1 = opt_val;
                                        break;
                                    case 1:
                                        sql_opt_2 = opt_name;
                                        sql_opt_val_2 = opt_val;
                                        break;
                                    case 2:
                                        sql_opt_3 = opt_name;
                                        sql_opt_val_3 = opt_val;
                                        break;
                                }
                            }
                        }
                        // 상품 옵션 테이블 한 줄에 3줄 나오는 템플릿 만들기
                        opts_with_img = doc.DocumentNode.SelectNodes("//dl[contains(@class, 'tm-img-prop')]/dd/ul/li/a");
                        if(opts_with_img == null)
                        {
                            opts_with_img = doc.DocumentNode.SelectNodes("//ul[contains(@class, 'tb-img')]/li/a");
                        }
                        if (opts_with_img != null)
                        {
                            int back_img_start = 0, back_img_end = 0;
                            string opt_back_url = "";
                            opt_imgs = new List<string>();
                            foreach(var item in opts_with_img)
                            {
                                if (item.Attributes["style"] == null)
                                {
                                    opt_imgs.Add(opt_back_url + "^^" + item.ChildNodes["span"].InnerText);
                                }
                                else if(item.Attributes["style"].Value.IndexOf(".jpg_") > -1 || item.Attributes["style"].Value.IndexOf(".png_") > -1)
                                {
                                    back_img_start = item.Attributes["style"].Value.IndexOf("url(");
                                    back_img_end = (item.Attributes["style"].Value.IndexOf(".jpg_") > -1 ? item.Attributes["style"].Value.IndexOf(".jpg_") : item.Attributes["style"].Value.IndexOf(".png_"));
                                    opt_back_url = item.Attributes["style"].Value.Substring(back_img_start + 4, (back_img_end - back_img_start));
                                    opt_imgs.Add(opt_back_url + "^^" + item.ChildNodes["span"].InnerText);
                                }
                            }
                        }
                        if (opt_imgs.Count > 0) sql_opt_imgs = string.Join(",", opt_imgs);
                        else sql_opt_imgs = "";

                        // 상품 재고 : J_SpanStock
                        stock = doc.DocumentNode.SelectNodes("//em[@id='J_EmStock']");
                        if (stock != null) sql_prd_stock = stock[0].InnerHtml;
                        else
                        {
                            stock = doc.DocumentNode.SelectNodes("//span[@id='J_SpanStock']");
                            if (stock != null) sql_prd_stock = stock[0].InnerHtml;
                        }
                        // 상품 세부 정보
                        attr = doc.DocumentNode.SelectNodes("//ul[@id='J_AttrUL']/li");
                        if (attr == null)
                        {
                            attr = doc.DocumentNode.SelectNodes("//div[@id='attributes']/ul/li");
                        }
                        if (attr != null)
                        {
                            List<string> attr_list = new List<string>();
                            foreach (HtmlNode item in attr)
                            {
                                attr_list.Add(item.InnerText.Replace("&nbsp;", ""));
                            }
                            sql_prd_attr = string.Join(",", attr_list);
                        }
                        // 제조사(brand)
                        prd_brand = doc.DocumentNode.SelectNodes("//div[@id='J_BrandAttr']/div/b");
                        if(prd_brand != null)
                        {
                            sql_prd_brand = prd_brand[0].InnerText;
                        }
                        // 상품 추가 이미지
                        additional_image = doc.DocumentNode.SelectNodes("//ul[@id='J_UlThumb']/li");
                        if (additional_image.Count > 0)
                        {
                            int idx = 0;
                            sql_add_img_1 = ""; sql_add_img_2 = ""; sql_add_img_3 = ""; sql_add_img_4 = "";
                            foreach (HtmlNode item in additional_image)
                            {
                                if (item.Id == "J_VideoThumb")
                                {

                                }
                                else
                                {
                                    idx++;
                                    switch (idx)
                                    {
                                        case 1:
                                            if (item.ChildNodes.Count == 1)
                                                sql_add_img_1 = "https://" + item.ChildNodes["a"].ChildNodes["img"].Attributes["src"].Value;
                                            else if (item.ChildNodes[1].Name == "a")
                                                sql_add_img_1 = "https://" + item.ChildNodes["a"].ChildNodes["img"].Attributes["src"].Value;
                                            else if (item.ChildNodes[1].Name == "div")
                                                sql_add_img_1 = "https://" + item.ChildNodes["div"].ChildNodes["a"].ChildNodes["img"].Attributes["src"].Value;
                                            break;
                                        case 2:
                                            if (item.ChildNodes.Count == 1)
                                                sql_add_img_2 = "https://" + item.ChildNodes["a"].ChildNodes["img"].Attributes["src"].Value;
                                            else if (item.ChildNodes[1].Name == "a")
                                                sql_add_img_2 = "https://" + item.ChildNodes["a"].ChildNodes["img"].Attributes["src"].Value;
                                            else if (item.ChildNodes[1].Name == "div")
                                                sql_add_img_2 = "https://" + item.ChildNodes["div"].ChildNodes["a"].ChildNodes["img"].Attributes["src"].Value;
                                            break;
                                        case 3:
                                            if (item.ChildNodes.Count == 1)
                                                sql_add_img_3 = "https://" + item.ChildNodes["a"].ChildNodes["img"].Attributes["src"].Value;
                                            else if (item.ChildNodes[1].Name == "a")
                                                sql_add_img_3 = "https://" + item.ChildNodes["a"].ChildNodes["img"].Attributes["src"].Value;
                                            else if (item.ChildNodes[1].Name == "div")
                                                sql_add_img_3 = "https://" + item.ChildNodes["div"].ChildNodes["a"].ChildNodes["img"].Attributes["src"].Value;
                                            break;
                                        case 4:
                                            if (item.ChildNodes.Count == 1)
                                                sql_add_img_4 = "https://" + item.ChildNodes["a"].ChildNodes["img"].Attributes["src"].Value;
                                            else if (item.ChildNodes[1].Name == "a")
                                                sql_add_img_4 = "https://" + item.ChildNodes["a"].ChildNodes["img"].Attributes["src"].Value;
                                            else if (item.ChildNodes[1].Name == "div")
                                                sql_add_img_4 = "https://" + item.ChildNodes["div"].ChildNodes["a"].ChildNodes["img"].Attributes["src"].Value;
                                            break;
                                    }
                                }
                            }
                        }

                        update_rows.Add(string.Format("('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}','{14}','{15}','{16}','{17}','{18}')",
                            MySqlHelper.EscapeString(sql_id),
                            MySqlHelper.EscapeString(sql_prd_price),
                            MySqlHelper.EscapeString(sql_prd_promo),
                            MySqlHelper.EscapeString(sql_prd_stock),
                            MySqlHelper.EscapeString(sql_opt_imgs),
                            MySqlHelper.EscapeString(sql_detail_img),
                            MySqlHelper.EscapeString(sql_prd_brand),
                            MySqlHelper.EscapeString(sql_opt_1),
                            MySqlHelper.EscapeString(sql_opt_val_1),
                            MySqlHelper.EscapeString(sql_opt_2),
                            MySqlHelper.EscapeString(sql_opt_val_2),
                            MySqlHelper.EscapeString(sql_opt_3),
                            MySqlHelper.EscapeString(sql_opt_val_3),
                            MySqlHelper.EscapeString(sql_prd_attr),
                            MySqlHelper.EscapeString(sql_add_img_1),
                            MySqlHelper.EscapeString(sql_add_img_2),
                            MySqlHelper.EscapeString(sql_add_img_3),
                            MySqlHelper.EscapeString(sql_add_img_4),
                            MySqlHelper.EscapeString(DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"))
                            )
                        );
                    }
                    catch (Exception e)
                    {
                        continue;
                    }
                }
                conn.Open();
                if (update_rows.Count > 0)
                {
                    // tmp 테이블이 있는지 확인
                    string isExist_tmp = "SELECT count(*) as cnt FROM information_schema.tables WHERE table_name = 'tmp' ; ";
                    MySqlCommand count = new MySqlCommand(isExist_tmp, conn);
                    int tmp_count = Convert.ToInt32(count.ExecuteScalar());
                    if (tmp_count > 0)
                    {
                        // tmp 테이블이 있을 경우 drop table
                        MySqlCommand dropTmp = new MySqlCommand("drop table tmp; ", conn);
                        dropTmp.ExecuteNonQuery();
                    }

                    // 업데이트 하기 위한 임시 테이블 생성
                    string tmpCreate = "CREATE TABLE tmp(" +
                            "id VARCHAR(20), " +
                            "prd_price VARCHAR(50), " +
                            "prd_promo VARCHAR(50), " +
                            "prd_stock VARCHAR(45), " +
                            "prd_opt_imgs VARCHAR(8000), " +
                            "detail_img VARCHAR(8000), " +
                            "prd_brand VARCHAR(100), " +
                            "opt_1 VARCHAR(45), " +
                            "opt_val_1 VARCHAR(500), " +
                            "opt_2 VARCHAR(45), " +
                            "opt_val_2 VARCHAR(500), " +
                            "opt_3 VARCHAR(45), " +
                            "opt_val_3 VARCHAR(500), " +
                            "prd_attr VARCHAR(1000), " +
                            "add_img_1 VARCHAR(200), " +
                            "add_img_2 VARCHAR(200), " +
                            "add_img_3 VARCHAR(200), " +
                            "add_img_4 VARCHAR(200), " +
                            "updated_date VARCHAR(45) " +
                        ") " +
                        "DEFAULT CHARACTER SET = utf8 " +
                        "COLLATE = utf8_bin;";
                    MySqlCommand myCmdTemp = new MySqlCommand(tmpCreate, conn);
                    myCmdTemp.CommandTimeout = 1000;
                    myCmdTemp.ExecuteNonQuery();

                    sUPDATE.Append(string.Join(",", update_rows));
                    sUPDATE.Append(";");
                    using (MySqlCommand myCmd = new MySqlCommand(sUPDATE.ToString(), conn))
                    {
                        myCmd.CommandType = CommandType.Text;
                        myCmd.CommandTimeout = 1000;
                        myCmd.ExecuteNonQuery();

                        myCmd.CommandText = string.Format(
                            "UPDATE tmp T INNER JOIN taobao_goods TB ON T.id = TB.id " +
                            "SET TB.prd_price = T.prd_price, TB.prd_promo = T.prd_promo, TB.prd_stock = T.prd_stock, TB.detail_yn = '1', TB.detail_img = T.detail_img, TB.prd_brand = T.prd_brand, " +
                            "TB.opt_1 = T.opt_1, TB.opt_val_1 = T.opt_val_1, TB.opt_2 = T.opt_2, TB.opt_val_2 = T.opt_val_2, TB.opt_3 = T.opt_3, TB.opt_val_3 = T.opt_val_3, TB.prd_attr = T.prd_attr, " +
                           "TB.add_img_1 = T.add_img_1, TB.add_img_2 = T.add_img_2, TB.add_img_3 = T.add_img_3, TB.add_img_4 = T.add_img_4, TB.prd_opt_imgs = T.prd_opt_imgs, " +
                            "TB.updated_date = T.updated_date ; " +
                            "DROP TABLE tmp;",
                            DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
                        );
                        myCmd.ExecuteNonQuery();
                    }
                }
                if (conn != null) conn.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                throw e;
            }
            finally
            {
                fnGenResult();
                MessageBox.Show("상품수집 완료" + Environment.NewLine + "수집성공 : " + success_ids.Count + "개" + Environment.NewLine + "수집실패 : " + error_ids.Count + "개");
                doc_opacity.Visibility = Visibility.Collapsed;
                doc_status.Visibility = Visibility.Collapsed;
                if (conn != null) conn.Close();
            }
        }
        // 상품정보를 taobao_goods 테이블에서 가져온다. 
        private void getDbData()
        {
            MySqlConnection conn = null;
            db_list = new ArrayList();
            try
            {
                conn = new MySqlConnection(strConn);
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conn;
                conn.Open();
                cmd.Prepare();
                cmd.CommandText = "SELECT * FROM taobao_goods; ";
                MySqlDataReader reader = cmd.ExecuteReader();
                string id;
                while (reader.Read())
                {
                    id = reader["id"] as string;

                     if (id_list.IndexOf(id) == -1)
                     {
                        p_id_list.Add(id);
                        db_list.Add(new Prd_Store(
                            reader["id"] as string,
                            reader["prd_img"] as string,
                            reader["prd_name"] as string,
                            reader["prd_attr"] as string,
                            reader["detail_yn"] as string,
                            reader["prd_price"] as string,
                            reader["prd_opt"] as string,
                            reader["detail_img"] as string,
                            reader["created_date"] as string,
                            reader["updated_date"] as string,
                            reader["user_id"] as string,
                            reader["prd_status"] as string
                            ));
                     }
                }
            }
            finally
            {
                if (conn != null) conn.Close();
            }
        }

        #region event collection
        // 파싱할 상품 정보 테이블 생성
        // 파싱하기
        private void btnParsor_Click(object sender, RoutedEventArgs e)
        {
            if (strHtml == "") return;
            getData(strHtml);
        }
        //  DB 저장 버튼 클릭 이벤트
        private void btnStoreDB_Click(object sender, RoutedEventArgs e)
        {
            // 카테고리가 선택되어 있는지 확인
            if (selectedCategory == null)
            {
                MessageBox.Show("카테고리를 선택해주세요.");
                return;
            }
            //ViewModel.CategoryViewModel category = (ViewModel.CategoryViewModel)cbCategory.SelectedItem;
            string catgory_code = string.Format("{0}{1}{2}{3}{4}", selectedCategory.Cate_type, selectedCategory.L, selectedCategory.M, selectedCategory.S, selectedCategory.XS);
            if (prdView_list == null) return;
            StringBuilder sINSERT = new StringBuilder("INSERT INTO taobao_goods(id, prd_img, prd_name, prd_category, created_date) VALUES ");
            StringBuilder sUPDATE = new StringBuilder("INSERT INTO tmp(id, prd_img, prd_name, prd_category, created_date) VALUES ");
            List<string> insert_rows = new List<string>();
            List<string> update_rows = new List<string>();
            using (MySqlConnection mConn = new MySqlConnection(strConn))
            {
                try
                {
                    foreach (ViewModel.ProductViewModel row in prdView_list)
                    {
                        if (row.Prd_exist != "O" & row.Prd_status != "9")
                        {
                            insert_rows.Add(string.Format("('{0}', '{1}', '{2}', '{3}', '{4}')",
                                MySqlHelper.EscapeString(row.Id),                              // 상품코드
                                MySqlHelper.EscapeString(row.Prd_img.ToString()),   // 상품이미지
                                MySqlHelper.EscapeString(row.Prd_name),                 // 상품명
                                MySqlHelper.EscapeString(catgory_code),           // 상품카테고리
                                //MySqlHelper.EscapeString(row.Prd_category),           // 상품카테고리
                                MySqlHelper.EscapeString(DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"))      // 생성일
                                )
                            );
                            row.Prd_exist = "O";
                            row.Prd_type = "[New!!]";
                        }
                        else
                        {
                            update_rows.Add(string.Format("('{0}', '{1}', '{2}', '{3}', '{4}')",
                                MySqlHelper.EscapeString(row.Id),                              // 상품코드
                                MySqlHelper.EscapeString(row.Prd_img.ToString()),   // 상품이미지
                                MySqlHelper.EscapeString(row.Prd_name),                 // 상품명
                                MySqlHelper.EscapeString(catgory_code),           // 상품카테고리
                                //MySqlHelper.EscapeString(row.Prd_category),           // 상품카테고리
                                MySqlHelper.EscapeString(DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"))      // 생성일
                                )
                            );
                            row.Prd_type = "[Updated]";
                        }
                    }
                    mConn.Open();
                    #region 신규 상품 등록
                    if(insert_rows.Count > 0)
                    {
                        sINSERT.Append(string.Join(",", insert_rows));
                        sINSERT.Append(";");
                        using (MySqlCommand myCmd = new MySqlCommand(sINSERT.ToString(), mConn))
                        {
                            myCmd.CommandType = CommandType.Text;
                            myCmd.CommandTimeout = 1000;
                            myCmd.ExecuteNonQuery();
                        }
                    }
                    #endregion

                    #region 기존 상품 UPDATE
                    if(update_rows.Count > 0)
                    {
                        string tmpCreate = "CREATE TABLE tmp(id VARCHAR(20), prd_img VARCHAR(200), prd_name VARCHAR(100), prd_category VARCHAR(45), created_date VARCHAR(45)) " +
                            "DEFAULT CHARACTER SET = utf8 " +
                            "COLLATE = utf8_bin;";
                        MySqlCommand myCmdTemp = new MySqlCommand(tmpCreate, mConn);
                        myCmdTemp.CommandTimeout = 1000;
                        myCmdTemp.ExecuteNonQuery();

                        sUPDATE.Append(string.Join(",", update_rows));
                        sUPDATE.Append(";");
                        using (MySqlCommand myCmd = new MySqlCommand(sUPDATE.ToString(), mConn))
                        {
                            myCmd.CommandType = CommandType.Text;
                            myCmd.CommandTimeout = 1000;
                            myCmd.ExecuteNonQuery();

                            myCmd.CommandText = string.Format(
                                "UPDATE tmp T INNER JOIN taobao_goods TB ON T.id = TB.id " +
                                "SET TB.prd_img = T.prd_img, TB.prd_name = T.prd_name, TB.prd_category = T.prd_category, TB.updated_date = '{0}'; " +
                                "DROP TABLE tmp;",
                                DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
                            );
                            myCmd.ExecuteNonQuery();
                        }
                    }

                    #endregion

                    MessageBox.Show(string.Format("저장 성공! \n신규 상품 {0}개 등록. \n기존 상품 {1}개 업데이트 되었습니다.", insert_rows.Count, update_rows.Count));
                }
                catch(Exception ex) 
                {
                    MessageBox.Show(ex.ToString());
                    throw ex;
                }
                finally
                {
                    if (mConn != null) mConn.Close();
                }
            }
        }
        // 상품정보 가져오기
        private void btnGetProductInfo_Click(object sender, RoutedEventArgs e)
        {
            int chk_count = 0;
            List<string> detail_urls = new List<string>();
            html_node = new List<string>();
            success_ids = new List<string>();
            error_ids = new List<Tuple<string, string>>();

            // Take screenshots of 3 web pages, and save the screenshots to C:\testX.png


            foreach (ViewModel.ProductViewModel prd in prdView_list)
            {
                if (prd.IsSelected)
                {
                    chk_count++;
                    detail_urls.Add(prd.Id);
                }
            }

            getDetailHtml(detail_urls);
        }
        #region puppeteer로 수집하는 것
        //    txt_crawling_status.Text = "상품 수집에 필요한 리소스를 받아오는 중입니다.";
        //    string url = "";
        //    try
        //    {



        //        foreach (ViewModel.ProductViewModel prd in prdView_list)
        //        {
        //            if (prd.IsSelected) chk_count++;
        //        }

        //        foreach (ViewModel.ProductViewModel prd in prdView_list)
        //        {
        //            try
        //            {
        //                if (prd.IsSelected)
        //                {
        //                    using (var page = await browser.NewPageAsync())
        //                    {
        //                        count++;
        //                        url = "https://detail.tmall.com/item.htm?id=" + prd.Id;
        //                        txt_crawling_count.Text = count.ToString() + "/" + chk_count.ToString() + "상품 수집 중";
        //                        txt_crawling_status.Text = url;
        //                        var response = await page.GoToAsync(url, new NavigationOptions
        //                        {
        //                            WaitUntil = new WaitUntilNavigation[]
        //                            {
        //                                WaitUntilNavigation.Networkidle0
        //                            }
        //                        });
        //                        var cookies = new CookieParam { Name = "", Value = "" };

        //                        await page.SetCookieAsync(new CookieParam
        //                        {
        //                            Name = "test",
        //                            Value = "1234"
        //                        });
        //                        await page.ClickAsync("#sufei-dialog-close");
        //                        await page.WaitForTimeoutAsync(4000);

        //                        //await page.GoToAsync(url);
        //                        //await page.SetCookieAsync(new CookieParam
        //                        //{
        //                        //    Name = "hng", Value = "KR % 7Czh - CN % 7CKRW % 7C410"
        //                        //});
        //                        //await page.WaitForSelectorAsync(".tm-promo-price");

        //                        html_node.Add(await page.GetContentAsync());
        //                        success_ids.Add(prd.Id);
        //                    }
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                error_ids.Add(Tuple.Create(prd.Id, ex.ToString()));
        //                continue;
        //                //throw ex;
        //            }
        //        }

        //        //await browser.CloseAsync();
        //    }
        //    catch (Exception exc)
        //    {
        //        throw exc;
        //    }
        //    finally
        //    {
        //        await browser.CloseAsync();
        //        // 상품 디테일 파싱하기
        //        parsingPrdDetail(html_node);
        //    }
        //}
        #endregion

        private async void getDetailHtml(List<string> detail_urls)
        {
            int count = 0;
            doc_opacity.Visibility = Visibility.Visible;
            doc_status.Visibility = Visibility.Visible;
            foreach (string id in detail_urls)
            {
                try
                {
                    count++;
                    txt_crawling_count.Text = count.ToString() + "/" + detail_urls.Count + "상품 수집 중";
                    txt_crawling_status.Text = id;

                    puppeteer_page = await pup_browser.NewPageAsync();
                    await puppeteer_page.GoToAsync("https://detail.tmall.com/item.htm?id=" + id);
                    await puppeteer_page.WaitForSelectorAsync("#tstart");
                    //var response = await puppeteer_page.GoToAsync("https://detail.tmall.com/item.htm?id=" + id, new NavigationOptions
                    //{
                    //    WaitUntil = new WaitUntilNavigation[]
                    //    {
                    //    WaitUntilNavigation.Networkidle0
                    //    }
                    //});
                    //await puppeteer_page.SetViewportAsync(new ViewPortOptions
                    //{
                    //    Width = 800,
                    //    Height = 2000
                    //});
                    html_node.Add(await puppeteer_page.GetContentAsync());
                    success_ids.Add(id);
                }
                catch (TimeoutException t_ex)
                {
                    error_ids.Add(Tuple.Create(id, t_ex.ToString()));
                    continue;
                }
                catch (Exception ex)
                {
                    error_ids.Add(Tuple.Create(id, ex.ToString()));
                    continue;
                }
                finally
                {
                    await puppeteer_page.CloseAsync();
                }
            }
            parsingPrdDetail(html_node);
        }

        private void BtnMyDB_Click(object sender, RoutedEventArgs e)
        {
            Window win_myDB = (Window)Application.LoadComponent(new Uri("myDB.xaml", UriKind.Relative));
            win_myDB.Visibility = win_myDB.Visibility == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;
        }
        // 전체 체크 이벤트
        private void chkSelectAll_Checked(object sender, RoutedEventArgs e)
        {
            foreach(ViewModel.ProductViewModel p in prdView_list)
            {
                p.IsSelected = true;
            }
        }
        // 전체 체크 해제 이벤트
        private void chkSelectAll_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (ViewModel.ProductViewModel p in prdView_list)
            {
                p.IsSelected = false;
            }
        }
        // Main 화면 탭
        private void BtnPrev_Click(object sender, RoutedEventArgs e)
        {
            main_doc.Visibility = Visibility.Visible;
            myDb_doc.Visibility = Visibility.Collapsed;
            doc_CtrlDelPrd.Visibility = Visibility.Collapsed;
            doc_config.Visibility = Visibility.Collapsed;
            cbCategory.ItemsSource = webCrawler.Contoller.Category.readCategory("A");
        }
        // DB List 탭 클릭
        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            main_doc.Visibility = Visibility.Collapsed;
            myDb_doc.Visibility = Visibility.Visible;
            doc_CtrlDelPrd.Visibility = Visibility.Collapsed;
            doc_config.Visibility = Visibility.Collapsed;
            cbDbListCategory.ItemsSource = webCrawler.Contoller.Category.readCategory("A");
        }
        // 제외 상품 관리 탭 클릭
        private void BtnCtrlDelPrd_Click(object sender, RoutedEventArgs e)
        {
            main_doc.Visibility = Visibility.Collapsed;
            myDb_doc.Visibility = Visibility.Collapsed;
            doc_CtrlDelPrd.Visibility = Visibility.Visible;
            doc_config.Visibility = Visibility.Collapsed;
            showDelPrdList();
        }
        // 환경설정 탭 클릭
        private void BtnConfig_Click(object sender, RoutedEventArgs e)
        {
            main_doc.Visibility = Visibility.Collapsed;
            myDb_doc.Visibility = Visibility.Collapsed;
            doc_CtrlDelPrd.Visibility = Visibility.Collapsed;
            doc_config.Visibility = Visibility.Visible;
            lvCategory_L.ItemsSource = webCrawler.Contoller.Category.readCategory("A");
            initCategoryView();
        }
        // Main 화면 이동 버튼 클릭 이벤트
        private void btn_main_Click(object sender, RoutedEventArgs e)
        {
            browser.Address = main_url;
        }
        // 카테고리 이동하기 버튼 클릭 이벤트
        private void category_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            string url = "";
            switch (btn.Content)
            {
                case "女装精品": url = "https://s.taobao.com/search?q=%E5%A5%B3%E8%A3%85"; break;
                case "内衣/男装": url = "https://s.taobao.com/search?q=男装"; break;
                case "鞋品/箱包": url = "https://s.taobao.com/search?q=鞋包"; break;
                case "饰品/配件": url = "https://s.taobao.com/search?q=饰品配件"; break;
                case "运动/户外": url = "https://s.taobao.com/search?q=运动户外"; break;
                case "家具/家电": url = "https://s.taobao.com/search?q=家具家电"; break;
                case "居家/家纺": url = "https://s.taobao.com/search?q=家居家纺"; break;
                case "手机/数码": url = "https://s.taobao.com/search?q=3c数码配件"; break;
                case "汽车/家电/旅行": url = "https://s.taobao.com/search?q=%E5%B0%8F%E5%AE%B6%E7%94%B5"; break;
                case "母婴/玩具": url = "https://s.taobao.com/search?cat=50067081"; break;
                case "办公/娱乐": url = "https://s.taobao.com/search?q=文具"; break;
                case "美妆/个护": url = "https://s.taobao.com/search?q=%E7%BE%8E%E5%AE%B9"; break;
            }
            browser.Address = url;
            clearDbTable();     // 목록비우기
        }
        #endregion

        #region MyDB & excel download
        List<ViewModel.MyDBViewModel> myDBView_list;
        //  상품정보 읽어오기
        private void getMyDB(string cate_id)
        {
            MySqlConnection conn = null;
            try
            {
                StringBuilder query_builder = new StringBuilder();
                query_builder.Append(string.Format(
                    "SELECT G.id as id, G.prd_img as prd_img , C.cate_name as prd_category, G.prd_name as prd_name, G.prd_attr as prd_attr, G.detail_yn as detail_yn, G.prd_price as prd_price, G.prd_promo as prd_promo, G.prd_brand as prd_brand, " +
                    "G.opt_1 as opt_1, G.opt_val_1 as opt_val_1, G.opt_2 as opt_2, G.opt_val_2 as opt_val_2, G.opt_3 as opt_3, G.opt_val_3 as opt_val_3, " +
                    "G.prd_opt_imgs as prd_opt_imgs, G.prd_stock as prd_stock, G.detail_img as detail_img, G.add_img_1 as add_img_1, G.add_img_2 as add_img_2, G.add_img_3 as add_img_3, G.add_img_4 as add_img_4, " +
                    "G.created_date as created_date, G.updated_date as updated_date, G.user_id as user_id, " +
                    "C.Id as cate_id " +
                    "FROM taobao_goods G " +
                    "LEFT OUTER JOIN taobao_category C ON G.prd_category = C.Id " +
                    "WHERE prd_status = '1' AND prd_category = '{0}'  ", cate_id));
                if (dpCreate_date_from.Text != "" && dpCreate_date_to.Text != "") query_builder.Append(string.Format("AND substr(created_date, 1, 10) BETWEEN '{0}' AND '{1}' ", dpCreate_date_from.Text, dpCreate_date_to.Text ));
                if (dpUpdate_date_from.Text != "" && dpUpdate_date_to.Text != "") query_builder.Append(string.Format("AND substr(updated_date, 1, 10) BETWEEN '{0}' AND '{1}' ", dpUpdate_date_from.Text, dpUpdate_date_to.Text ));
                query_builder.Append(";");

                myDBView_list = new List<ViewModel.MyDBViewModel>();
                conn = new MySqlConnection(strConn);
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conn;
                conn.Open();
                cmd.Prepare();
                cmd.CommandText = query_builder.ToString();
                MySqlDataReader reader = cmd.ExecuteReader();
                BitmapImage bi = new BitmapImage();
                int num = 1;
                while (reader.Read())
                {
                    bi = new BitmapImage();
                    bi.BeginInit();
                    bi.UriSource = new Uri(reader["prd_img"] as string, UriKind.RelativeOrAbsolute);
                    bi.EndInit();
                    myDBView_list.Add(new ViewModel.MyDBViewModel(
                        false, num++, reader["id"].ToString(), bi, reader["prd_category"].ToString(), 
                        reader["prd_name"].ToString(), reader["prd_attr"].ToString(),reader["detail_yn"].ToString(), 
                        reader["prd_price"].ToString(), reader["prd_promo"].ToString(), reader["prd_brand"].ToString(),
                        reader["opt_1"].ToString(), reader["opt_val_1"].ToString(), reader["opt_2"].ToString(), reader["opt_val_2"].ToString(), reader["opt_3"].ToString(), reader["opt_val_3"].ToString(),
                        reader["prd_opt_imgs"].ToString(), reader["prd_stock"].ToString(), reader["detail_img"].ToString(), 
                        reader["add_img_1"].ToString(), reader["add_img_2"].ToString(), reader["add_img_3"].ToString(), reader["add_img_4"].ToString(),
                        reader["created_date"].ToString(), reader["updated_date"].ToString(), reader["user_id"].ToString()
                        )
                    );
                }
                dgMyDB.ItemsSource = myDBView_list;
            }
            finally
            {
                if (conn != null) conn.Close();
            }
        }
        // DB List 전체 체크
        private void ChkMyDb_Checked(object sender, RoutedEventArgs e)
        {
            foreach (ViewModel.MyDBViewModel row in myDBView_list)
            {
                row.IsSelected = true;
            }
        }
        // DB List 전체 체크 해제
        private void ChkMyDb_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (ViewModel.MyDBViewModel row in myDBView_list)
            {
                row.IsSelected = false;
            }
        }
        // 상품 수집 제외
        private void BtnDelProduct_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder sINSERT = new StringBuilder("INSERT INTO _tmp (id) VALUES  ");
            List<string> insert_rows = new List<string>();
            MySqlConnection conn = null;
            foreach(ViewModel.MyDBViewModel item in myDBView_list)
            {
                if (item.IsSelected) insert_rows.Add(string.Format("( '{0}' )", MySqlHelper.EscapeString(item.Id)));
            }
            if(insert_rows.Count == 0) MessageBox.Show("수집 제외할 상품을 선택해주세요.");
            else
            {
                try
                {
                    conn = new MySqlConnection(strConn);
                    conn.Open();

                    string tmpCreate = "CREATE TABLE _tmp(id VARCHAR(20) ) DEFAULT CHARACTER SET = utf8 COLLATE = utf8_bin; ";
                    MySqlCommand myCmdTemp = new MySqlCommand(tmpCreate, conn);
                    myCmdTemp.CommandType = CommandType.Text;
                    myCmdTemp.CommandTimeout = 1000;
                    myCmdTemp.ExecuteNonQuery();

                    sINSERT.Append(string.Join(",", insert_rows));
                    sINSERT.Append(";");
                    using (MySqlCommand myCmd = new MySqlCommand(sINSERT.ToString(), conn))
                    {
                        myCmd.CommandType = CommandType.Text;
                        myCmd.CommandTimeout = 1000;
                        myCmd.ExecuteNonQuery();

                        myCmd.CommandText = string.Format(
                            "UPDATE taobao_goods T INNER JOIN _tmp A ON T.id = A.id " +
                            "SET T.prd_status = '9'; " +
                            "DROP TABLE _tmp; "
                        ) ;
                        myCmd.ExecuteNonQuery();

                        MessageBox.Show(string.Format("총 {0}개의 상품이 수집제외 됐습니다.", insert_rows.Count));
                        ViewModel.CategoryViewModel item = (ViewModel.CategoryViewModel)cbDbListCategory.SelectedItem;
                        if(item != null) getMyDB(item.Id.ToString());
                    }
                }catch(Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                    throw ex;
                }
                finally
                {
                    if (conn != null) conn.Close();
                    clearDbTable();
                    getDbData();
                }
            }
        }
        // 목록비우기 클릭
        private void BtnClearPrdList_Click(object sender, RoutedEventArgs e)
        {
            clearDbTable();     // 목록비우기
            getDbData();        // 상품정보 읽어오기
        }
        // 목록비우기
        private void clearDbTable()
        {
            idx = 1;
            prdView_list = new List<ViewModel.ProductViewModel>(); ;
            dgTable.ItemsSource = null;
            id_list = new List<string>();
        }
        // 제외상품 리스트 생성
        private void showDelPrdList()
        {
            delPrdView_list = new List<ViewModel.delProductViewModel>();
            MySqlConnection conn = null;
            try
            {
                conn = new MySqlConnection(strConn);
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conn;
                conn.Open();
                cmd.Prepare();
                cmd.CommandText = "SELECT * FROM taobao_goods WHERE prd_status = '9'; ";
                MySqlDataReader reader = cmd.ExecuteReader();
                BitmapImage bi = new BitmapImage();
                int num = 1;
                while (reader.Read())
                {
                    bi = new BitmapImage();
                    bi.BeginInit();
                    bi.UriSource = new Uri(reader["prd_img"] as string, UriKind.RelativeOrAbsolute);
                    bi.EndInit();
                    delPrdView_list.Add(new ViewModel.delProductViewModel(
                        false, num++, reader["id"].ToString(), bi, reader["prd_name"].ToString()
                        )
                    );
                }
                dgCtrlDelPrd.ItemsSource = delPrdView_list;
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
                throw ex;
            }
            finally
            {
                if (conn != null) conn.Close();
            }
        }
        // 제외상품 전체 체크
        private void ChkCtrlDel_Checked(object sender, RoutedEventArgs e)
        {
            foreach(ViewModel.delProductViewModel row in delPrdView_list)
            {
                row.IsSelected = true;
            }
        }
        // 제외상품 전체 체크 해제
        private void ChkCtrlDel_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (ViewModel.delProductViewModel row in delPrdView_list)
            {
                row.IsSelected = false;
            }
        }
        // 상품제외 해제하기
        private void BtnReleaseProduct_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder sINSERT = new StringBuilder("INSERT INTO _tmp (id) VALUES  ");
            List<string> insert_rows = new List<string>();
            MySqlConnection conn = null;
            foreach (ViewModel.delProductViewModel item in delPrdView_list)
            {
                if (item.IsSelected) insert_rows.Add(string.Format("( '{0}' )", MySqlHelper.EscapeString(item.Id)));
            }
            if (insert_rows.Count == 0) MessageBox.Show("수집 제외할 상품을 선택해주세요.");
            else
            {
                try
                {
                    conn = new MySqlConnection(strConn);
                    conn.Open();

                    string tmpCreate = "CREATE TABLE _tmp(id VARCHAR(20) ) DEFAULT CHARACTER SET = utf8 COLLATE = utf8_bin; ";
                    MySqlCommand myCmdTemp = new MySqlCommand(tmpCreate, conn);
                    myCmdTemp.CommandType = CommandType.Text;
                    myCmdTemp.CommandTimeout = 1000;
                    myCmdTemp.ExecuteNonQuery();

                    sINSERT.Append(string.Join(",", insert_rows));
                    sINSERT.Append(";");
                    using (MySqlCommand myCmd = new MySqlCommand(sINSERT.ToString(), conn))
                    {
                        myCmd.CommandType = CommandType.Text;
                        myCmd.CommandTimeout = 1000;
                        myCmd.ExecuteNonQuery();

                        myCmd.CommandText = string.Format(
                            "UPDATE taobao_goods T INNER JOIN _tmp A ON T.id = A.id " +
                            "SET T.prd_status = '1'; " +
                            "DROP TABLE _tmp; "
                        );
                        myCmd.ExecuteNonQuery();

                        MessageBox.Show(string.Format("총 {0}개의 상품이 수집제외에서 해제 됐습니다.", insert_rows.Count));
                        ViewModel.CategoryViewModel item = (ViewModel.CategoryViewModel)cbDbListCategory.SelectedItem;
                        if(item != null) getMyDB(item.Id.ToString());
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                    throw ex;
                }
                finally
                {
                    if (conn != null) conn.Close();
                    clearDbTable();
                    showDelPrdList();
                }
            }
        }
        // 엑셀다운로드
        private void BtnDownloadExcel_Click(object sender, RoutedEventArgs e)
        {
            float exChange;
            if(float.TryParse(txtExchange.Text, out exChange))
            {
                webCrawler.Contoller.ExcelDownLoad.fnExcelDownLoad(dgMyDB, myDBView_list, float.Parse(txtExchange.Text));
            }
            else
            {
                MessageBox.Show("환율 값을 정확히 입력해주세요.");
            }
        }
        #endregion
        // 수집결과 출력
        public void fnGenResult()
        {
            dg_result.ItemsSource = null;
            foreach(var item in error_ids)
            {
                result_view_list.Add(new ViewModel.ResultViewModel(
                    item.Item1,     // 상품코드
                    item.Item2      // 수집결과
                    )
                );
            }
            dg_result.ItemsSource = result_view_list;
        }
        // MyDB List 상품 재수집하기
        private void BtnReCollection_Click(object sender, RoutedEventArgs e)
        {
            int chk_count = 0;
            List<string> detail_urls = new List<string>();
            html_node = new List<string>();
            success_ids = new List<string>();
            error_ids = new List<Tuple<string, string>>();

            foreach (ViewModel.MyDBViewModel row in myDBView_list)
            {
                if (row.IsSelected)
                {
                    chk_count++;
                    detail_urls.Add(row.Id);
                }
            }
            getDetailHtml(detail_urls);
            ViewModel.CategoryViewModel item = (ViewModel.CategoryViewModel)cbDbListCategory.SelectedItem;
            if(item != null) getMyDB(item.Id.ToString());
        }
        #region 카테고리관련 이벤트 모음
        // 카테고리 입력 : '대분류/중분류/소분류/세분류', 품목고시코드 : '135'
        private void BtnCategoryAdd_Click(object sender, RoutedEventArgs e)
        {
            if(txtCategoryName.Text != "")
            {
                webCrawler.Contoller.Category.createCategory(txtCategoryName.Text, txtCategoryDesc.Text, txtCategoryCode.Text);
                //txtCategoryName.Text = ""; txtCategoryDesc.Text = "";
                //lvCategory_L.ItemsSource = webCrawler.Contoller.Category.readCategory("A");
                //initCategoryView();
            }
            else
            {
                MessageBox.Show("카테고리 값은 필수 입력 값입니다. ");
            }
        }


        private void BtnCategoryUpdate_Click(object sender, RoutedEventArgs e)
        {
            webCrawler.Contoller.Category.updateCategory(txtCategoryId.Text, txtCategoryCode.Text, txtCategoryDesc.Text);
            //lvCategory_L.ItemsSource = webCrawler.Contoller.Category.readCategory("A");
            initCategoryView();
        }
        private void BtnCategoryDelete_Click(object sender, RoutedEventArgs e)
        {
            webCrawler.Contoller.Category.deleteCategory(txtCategoryId.Text);
            lvCategory_L.ItemsSource = webCrawler.Contoller.Category.readCategory("A");

        }
        private void ListViewConfigCategory_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //int idx = ListViewConfigCategory.SelectedIndex;
            //if(idx > -1)
            //{
            //    ViewModel.CategoryViewModel item = (ViewModel.CategoryViewModel)ListViewConfigCategory.SelectedItems[0];
            //    txtCategoryName.Text = item.Cate_name;
            //    txtCategoryDesc.Text = item.Cate_desc;
            //    txtCategoryId.Text = item.Id.ToString();
            //}
        }
        #endregion

        private void BtnGetMyDB_Click(object sender, RoutedEventArgs e)
        {
            if (selectedCategory != null)
            {
                string category_id = string.Format("{0}{1}{2}{3}{4}", selectedCategory.Cate_type, selectedCategory.L, selectedCategory.M, selectedCategory.S, selectedCategory.XS);
                getMyDB(category_id);
            }
            else
            {
                MessageBox.Show("카테고리를 선택해주세요.");
            }

            //ViewModel.CategoryViewModel item = (ViewModel.CategoryViewModel)cbDbListCategory.SelectedItem;

            //if (item != null) getMyDB(item.Id.ToString());
            
        }
        #region 환경설정 카테고리 ListView 선택 이벤트
        private void LvCategory_L_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listbox = (ListBox)sender;
            var list = (ViewModel.CategoryViewModel)listbox.SelectedItem;
            lvCategory_M.ItemsSource = webCrawler.Contoller.Category.readCategory("B", list);
            lvCategory_S.ItemsSource = null;
            lvCategory_XS.ItemsSource = null;
            setCateVal();
        }
        private void LvCategory_M_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listbox = (ListBox)sender;
            var list = (ViewModel.CategoryViewModel)listbox.SelectedItem;
            lvCategory_S.ItemsSource = webCrawler.Contoller.Category.readCategory("C", list);
            lvCategory_XS.ItemsSource = null;
            setCateVal();
        }

        private void LvCategory_S_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listbox = (ListBox)sender;
            var list = (ViewModel.CategoryViewModel)listbox.SelectedItem;
            lvCategory_XS.ItemsSource = webCrawler.Contoller.Category.readCategory("D", list);
            setCateVal();
        }
        private void LvCategory_XS_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //var listbox = (ListBox)sender;
            //var list = (ViewModel.CategoryViewModel)listbox.SelectedItem;
            //lvCategory_M.ItemsSource = webCrawler.Contoller.Category.readCategory("B", list);
            setCateVal();
        }
        #endregion
        private void setCateVal()
        {
            StringBuilder cate_name = new StringBuilder("");
            StringBuilder cate_code = new StringBuilder("");
            ViewModel.CategoryViewModel cate_l = null, cate_m = null, cate_s = null, cate_xs = null;
            string code = "";
            int cate_id = 0;
            if (lvCategory_L.SelectedValue != null)
            {
                cate_l = (ViewModel.CategoryViewModel)lvCategory_L.SelectedValue;
                cate_name.Append(cate_l.Cate_name);
                cate_code.Append(cate_l.L);
                code = cate_l.CODE;
                cate_id = cate_l.Id;
            }
            if (lvCategory_M.SelectedValue != null)
            {
                cate_m = (ViewModel.CategoryViewModel)lvCategory_M.SelectedValue;
                cate_name.Append(string.Format("/{0}", cate_m.Cate_name));
                cate_code.Append(string.Format("/{0}", cate_m.M));
                code = cate_m.CODE;
                cate_id = cate_m.Id;
            }
            if (lvCategory_S.SelectedValue != null)
            {
                cate_s = (ViewModel.CategoryViewModel)lvCategory_S.SelectedValue;
                cate_name.Append(string.Format("/{0}", cate_s.Cate_name));
                cate_code.Append(string.Format("/{0}", cate_s.S));
                code = cate_s.CODE;
                cate_id = cate_s.Id;
            }
            if (lvCategory_XS.SelectedValue != null)
            {
                cate_xs = (ViewModel.CategoryViewModel)lvCategory_XS.SelectedValue;
                cate_name.Append(string.Format("/{0}", cate_xs.Cate_name));
                cate_code.Append(string.Format("/{0}", cate_xs.XS));
                code = cate_xs.CODE;
                cate_id = cate_xs.Id;
            }
            txtCategoryName.Text = cate_name.ToString();
            txtCategoryCode.Text = cate_code.ToString();
            txtCategoryDesc.Text = code;
            txtCategoryId.Text = cate_id.ToString();
        }

        private void initCategoryView()
        {
            lvCategory_M.ItemsSource = null;
            lvCategory_S.ItemsSource = null;
            lvCategory_XS.ItemsSource = null;
        }
        #region 메인화면 카테고리 콤보박스 선택 이벤트
        private void CbCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = (ComboBox)sender;
            var selectedItem = (ViewModel.CategoryViewModel)comboBox.SelectedItem;
            cbCategory_M.ItemsSource = webCrawler.Contoller.Category.readCategory("B", selectedItem);
            cbCategory_S.ItemsSource = null;
            cbCategory_XS.ItemsSource = null;
            selectedCategory = selectedItem;
        }

        private void CbCategory_M_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = (ComboBox)sender;
            var selectedItem = (ViewModel.CategoryViewModel)comboBox.SelectedItem;
            cbCategory_S.ItemsSource = webCrawler.Contoller.Category.readCategory("C", selectedItem);
            cbCategory_XS.ItemsSource = null;
            selectedCategory = selectedItem;
        }

        private void CbCategory_S_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = (ComboBox)sender;
            var selectedItem = (ViewModel.CategoryViewModel)comboBox.SelectedItem;
            cbCategory_XS.ItemsSource = webCrawler.Contoller.Category.readCategory("D", selectedItem);
            selectedCategory = selectedItem;
        }

        private void CbCategory_XS_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = (ComboBox)sender;
            var selectedItem = (ViewModel.CategoryViewModel)comboBox.SelectedItem;
            selectedCategory = selectedItem;
        }
        #endregion
        #region DB List 화면 카테고리 콤보박스 선택 이벤트
        private void CbDbListCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = (ComboBox)sender;
            var selectedItem = (ViewModel.CategoryViewModel)comboBox.SelectedItem;
            cbDbListCategory_M.ItemsSource = webCrawler.Contoller.Category.readCategory("B", selectedItem);
            cbDbListCategory_S.ItemsSource = null;
            cbDbListCategory_XS.ItemsSource = null;
            selectedCategory = selectedItem;
        }

        private void CbDbListCategory_M_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = (ComboBox)sender;
            var selectedItem = (ViewModel.CategoryViewModel)comboBox.SelectedItem;
            cbDbListCategory_S.ItemsSource = webCrawler.Contoller.Category.readCategory("C", selectedItem);
            cbDbListCategory_XS.ItemsSource = null;
            selectedCategory = selectedItem;
        }

        private void CbDbListCategory_S_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = (ComboBox)sender;
            var selectedItem = (ViewModel.CategoryViewModel)comboBox.SelectedItem;
            cbDbListCategory_XS.ItemsSource = webCrawler.Contoller.Category.readCategory("D", selectedItem);
            selectedCategory = selectedItem;
        }

        private void CbDbListCategory_XS_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = (ComboBox)sender;
            var selectedItem = (ViewModel.CategoryViewModel)comboBox.SelectedItem;
            selectedCategory = selectedItem;
        }
        #endregion
        // 시스템이 종료될 때 puppeteer도 함께 종료시킴.
        private async void Window_Closed(object sender, EventArgs e)
        {
            if (!puppeteer_page.IsClosed) await puppeteer_page.CloseAsync();
            if (!pup_browser.IsClosed) pup_browser.Dispose();
        }
    }
}
