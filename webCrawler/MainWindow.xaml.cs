using CefSharp;
using CefSharp.Wpf;
using HtmlAgilityPack;
using MySql.Data.MySqlClient;
using PuppeteerSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using NsExcel = Microsoft.Office.Interop.Excel;

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

        // 상품리스트 파싱하기 & prd_list 에 저장하기
        ArrayList prd_list = new ArrayList();           // Product 클래스를 담아두는 ArrayList
        List<string> id_list = new List<string>();      // 상품 중복 파싱을 방지하기 위한 상품코드 저장하는 List
        List<ViewModel.ProductViewModel> prdView_list = new List<ViewModel.ProductViewModel>();
        List<ViewModel.ResultViewModel> result_view_list = new List<ViewModel.ResultViewModel>();
        int idx = 1;

        // 제외상품 리스트 관련 변수
        List<ViewModel.delProductViewModel> delPrdView_list = new List<ViewModel.delProductViewModel>();

        CefSettings settings = new CefSettings();

        public MainWindow()
        {
            InitializeComponent();
            loginTaoBao("supereggsong", "alsdud1218!");
            this.DataContext = new ViewModel.ProductViewModel();
            InitializeChromium();
            getDbData();
        }
        // puppeteer 로그인
        private async void loginTaoBao(string id, string pwd)
        {
            try
            {
                string test_url = "https://intoli.com/blog/not-possible-to-block-chrome-headless/chrome-headless-test.html";
                string chrome_exe_path = "C:\\Program Files (x86)\\Google\\Chrome\\Application\\chrome.exe";
                doc_opacity.Visibility = Visibility.Visible;
                doc_status.Visibility = Visibility.Visible;
                txt_crawling_count.Text = "프로그램 작동에 필요한 리소스를 수집하는 중입니다.";
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
                var browser = await Puppeteer.LaunchAsync(new LaunchOptions
                {
                    Headless = true,
                    Args = args,
                    IgnoreHTTPSErrors = true,
                    ExecutablePath = chrome_exe_path
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
                puppeteer_page = await browser.NewPageAsync();
                await puppeteer_page.SetUserAgentAsync("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/75.0.3770.142 Safari/537.36");
                await puppeteer_page.SetExtraHttpHeadersAsync(header);
                await puppeteer_page.EvaluateFunctionOnNewDocumentAsync(overrideNavigatorWebdriver);
                await puppeteer_page.EvaluateFunctionOnNewDocumentAsync(overridePermission);
                await puppeteer_page.EvaluateFunctionOnNewDocumentAsync(overrideChrome);
                await puppeteer_page.EvaluateFunctionOnNewDocumentAsync(overridePlugin);
                await puppeteer_page.GoToAsync(login_frame_url, new NavigationOptions
                {
                     WaitUntil = new WaitUntilNavigation[]
                     {
                        WaitUntilNavigation.Load
                     }
                });
                await puppeteer_page.ScreenshotAsync("d:\\screenshot.png");
                await puppeteer_page.TypeAsync("#TPL_username_1", "supereggsong");
                await puppeteer_page.TypeAsync("#TPL_password_1", "alsdud1218!");
                var slider = await puppeteer_page.QuerySelectorAsync("#nc_1_wrapper");
                if (slider != null)
                {
                    await puppeteer_page.Mouse.MoveAsync(260, 210);
                    await puppeteer_page.Mouse.DownAsync();
                    await puppeteer_page.Mouse.MoveAsync(550, 210);
                    await puppeteer_page.Mouse.UpAsync();
                }
                await puppeteer_page.ClickAsync("#J_SubmitStatic");
                await puppeteer_page.WaitForTimeoutAsync(5000);
                var current_url = puppeteer_page.Url;
                if(current_url.IndexOf("login") > -1)
                {
                    await puppeteer_page.TypeAsync("#TPL_username_1", "supereggsong");
                    await puppeteer_page.TypeAsync("#TPL_password_1", "alsdud1218!");
                    if (slider != null)
                    {
                        await puppeteer_page.Mouse.MoveAsync(260, 210);
                        await puppeteer_page.Mouse.DownAsync();
                        await puppeteer_page.Mouse.MoveAsync(550, 210);
                        await puppeteer_page.Mouse.UpAsync();
                    }
                }
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
            browser.Address = login_url;
            
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

        private void parsingPrdDetail(List<string> html_node)
        {
            HtmlDocument doc = null, doc_img = null;
            HtmlNodeCollection img_wrap = null, imgs = null, price = null, promo = null, opts = null, stock = null, attr = null, additional_image = null;
            string[] strImg = null;

            StringBuilder sUPDATE = new StringBuilder("INSERT INTO tmp(id, prd_price, prd_promo, prd_stock, detail_img, " +
                "opt_1, opt_val_1, opt_2, opt_val_2, opt_3, opt_val_3, prd_attr, add_img_1, add_img_2, add_img_3, add_img_4, updated_date) VALUES ");
            string sql_id = "", sql_prd_price = "", sql_prd_promo = "", sql_prd_stock = "", sql_detail_img = "";
            string sql_opt_1 = "", sql_opt_val_1 = "", sql_opt_2 = "", sql_opt_val_2 = "", sql_opt_3 = "", sql_opt_val_3 = "", sql_prd_attr = "",
                sql_add_img_1 = "", sql_add_img_2 ="", sql_add_img_3 = "", sql_add_img_4 = "";
            List<string> update_rows = new List<string>();

            MySqlConnection conn = null;

            try
            {
                txt_crawling_status.Text = "DB 저장 중";
                conn = new MySqlConnection(strConn);

                foreach (var node in html_node)
                {
                    if (node == null) continue;
                    // 상품 속성 : prd_attr
                    doc = new HtmlDocument();
                    doc.LoadHtml(node);

                    sql_id = doc.DocumentNode.SelectNodes("//div[@id='LineZing']")[0].Attributes["itemid"].Value;

                    // 상품 상세 이미지 : prd_img
                    img_wrap = doc.DocumentNode.SelectNodes("//div[contains(@class, 'ke-post')]");
                    if (img_wrap != null)
                    {
                        doc_img = new HtmlDocument();
                        doc_img.LoadHtml(img_wrap[0].InnerHtml);
                        imgs = doc_img.DocumentNode.SelectNodes("//img[@data-ks-lazyload]");
                        if (imgs != null)
                        {
                            strImg = new string[imgs.Count];
                            for (var i = 0; i < imgs.Count; i++)
                            {
                                strImg[i] = string.Format("<img src='{0}'>",  imgs[i].Attributes["data-ks-lazyload"].Value);
                            }
                            //sql_detail_img = String.Join("&$%", strImg);
                            sql_detail_img = String.Join("", strImg);
                        }
                    }
                    // 타오바오 상품가격 : prd_price
                    price = doc.DocumentNode.SelectNodes("//dl[@class='tm-price-panel']/dd/span");
                    if (price != null) sql_prd_price = price[0].InnerText;

                    // 프로모션 가격(할인된 가격, 판매가격) : prd_promo
                    promo = doc.DocumentNode.SelectNodes("//div[@class='tm-promo-price']/span");
                    if (promo != null) sql_prd_promo = promo[0].InnerText;

                    // 상품 옵션
                    opts = doc.DocumentNode.SelectNodes("//dl[contains(@class, 'tm-sale-prop')]");
                    if (opts != null)
                    {
                        List<string> str_opts_value = new List<string>(); // 옵션 값을 담아두는 변수 선언
                        string opt_val = "";    // 옵션명을 담아두는 변수
                        for (var i = 0; i < opts.Count; i++)
                        {
                            string opt_name = opts[i].ChildNodes["dt"].InnerHtml;
                            HtmlNodeCollection opt_vals = opts[i].ChildNodes["dd"].ChildNodes["ul"].SelectNodes("li");
                            str_opts_value = new List<string>();
                            foreach (HtmlNode child in opt_vals)
                            {
                                str_opts_value.Add(child.ChildNodes["a"].ChildNodes["span"].InnerText);
                            }
                            opt_val = string.Join( ",", str_opts_value); //  옵션값 -> 콤마(,)로 구분함.
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

                    // 상품 재고 : J_SpanStock
                    stock = doc.DocumentNode.SelectNodes("//em[@id='J_EmStock']");
                    if(stock != null) sql_prd_stock = stock[0].InnerHtml;

                    // 상품 세부 정보
                    attr = doc.DocumentNode.SelectNodes("//ul[@id='J_AttrUL']/li");
                    if (attr != null)
                    {
                        List<string> attr_list = new List<string>();
                        foreach (HtmlNode item in attr)
                        {
                            attr_list.Add(item.InnerText.Replace("&nbsp;", ""));
                        }
                        sql_prd_attr = string.Join(",", attr_list);
                    }
                    // 상품 추가 이미지
                    additional_image = doc.DocumentNode.SelectNodes("//ul[@id='J_UlThumb']/li/a/img");
                    if(additional_image.Count > 0)
                    {
                        int idx = 0;
                        sql_add_img_1 = ""; sql_add_img_2 = ""; sql_add_img_3 = ""; sql_add_img_4 = "";
                        foreach (HtmlNode item in additional_image)
                        {
                            idx++;
                            switch (idx)
                            {
                                case 1: sql_add_img_1 = item.OuterHtml; break;
                                case 2: sql_add_img_2 = item.OuterHtml; break;
                                case 3: sql_add_img_3 = item.OuterHtml; break;
                                case 4: sql_add_img_4 = item.OuterHtml; break;
                            }
                        }
                    }
                    update_rows.Add(string.Format("('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}','{14}','{15}','{16}')",
                        MySqlHelper.EscapeString(sql_id), 
                        MySqlHelper.EscapeString(sql_prd_price),
                        MySqlHelper.EscapeString(sql_prd_promo),
                        MySqlHelper.EscapeString(sql_prd_stock),
                        MySqlHelper.EscapeString(sql_detail_img),
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
                conn.Open();
                if(update_rows.Count > 0)
                {
                    // 업데이트 하기 위한 임시 테이블 생성
                    string tmpCreate = "CREATE TABLE tmp(" +
                            "id VARCHAR(20), " +
                            "prd_price VARCHAR(50), " +
                            "prd_promo VARCHAR(50), " +
                            "prd_stock VARCHAR(45), " +
                            "detail_img VARCHAR(8000), " +
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
                            "SET TB.prd_price = T.prd_price, TB.prd_promo = T.prd_promo, TB.prd_stock = T.prd_stock, TB.detail_yn = '1', TB.detail_img = T.detail_img, " +
                            "TB.opt_1 = T.opt_1, TB.opt_val_1 = T.opt_val_1, TB.opt_2 = T.opt_2, TB.opt_val_2 = T.opt_val_2, TB.opt_3 = T.opt_3, TB.opt_val_3 = T.opt_val_3, TB.prd_attr = T.prd_attr, " +
                           "TB.add_img_1 = T.add_img_1, TB.add_img_2 = T.add_img_2, TB.add_img_3 = T.add_img_3, TB.add_img_4 = T.add_img_4, " +
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
                                MySqlHelper.EscapeString(row.Prd_category),           // 상품카테고리
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
                                MySqlHelper.EscapeString(row.Prd_category),           // 상품카테고리
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
                    var response = await puppeteer_page.GoToAsync("https://detail.tmall.com/item.htm?id=" + id, new NavigationOptions
                    {
                        WaitUntil = new WaitUntilNavigation[]
                        {
                        WaitUntilNavigation.Networkidle0
                        }
                    });
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

        private void BtnPrev_Click(object sender, RoutedEventArgs e)
        {
            main_doc.Visibility = Visibility.Visible;
            myDb_doc.Visibility = Visibility.Collapsed;
            doc_CtrlDelPrd.Visibility = Visibility.Collapsed;
        }

        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            main_doc.Visibility = Visibility.Collapsed;
            myDb_doc.Visibility = Visibility.Visible;
            doc_CtrlDelPrd.Visibility = Visibility.Collapsed;
            getMyDB();
        }
        // 제외 상품 관리 버튼 클릭
        private void BtnCtrlDelPrd_Click(object sender, RoutedEventArgs e)
        {
            main_doc.Visibility = Visibility.Collapsed;
            myDb_doc.Visibility = Visibility.Collapsed;
            doc_CtrlDelPrd.Visibility = Visibility.Visible;
            showDelPrdList();
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
        }
        #endregion

        #region MyDB & excel download
        List<ViewModel.MyDBViewModel> myDBView_list;
        //  상품정보 읽어오기
        private void getMyDB()
        {
            MySqlConnection conn = null;
            try
            {
                myDBView_list = new List<ViewModel.MyDBViewModel>();
                conn = new MySqlConnection(strConn);
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conn;
                conn.Open();
                cmd.Prepare();
                cmd.CommandText = "SELECT * FROM taobao_goods WHERE prd_status = '1'; ";
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
                        reader["prd_price"].ToString(), reader["prd_promo"].ToString(),
                        reader["opt_1"].ToString(), reader["opt_val_1"].ToString(), reader["opt_2"].ToString(), reader["opt_val_2"].ToString(), reader["opt_3"].ToString(), reader["opt_val_3"].ToString(),
                        reader["prd_stock"].ToString(), reader["detail_img"].ToString(), 
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
                        getMyDB();
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
                        getMyDB();
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
            var excel = new NsExcel.Application();

            excel.Visible = false;
            excel.DisplayAlerts = false;

            var excelWorkBook = excel.Workbooks.Add(Type.Missing);
            var excelSheet = (NsExcel.Worksheet)excelWorkBook.ActiveSheet;
            try
            {
                int r = dgMyDB.Items.Count;
                //int c = dgMyDB.Columns.Count;
                int c = 21;
                var data = new object[r, c];

                // 헤더 설정
                data[0, 0] = "순서";
                data[0, 1] = "상품코드";
                data[0, 2] = "상품명";
                data[0, 3] = "공급가격";
                data[0, 4] = "판매가격";
                data[0, 5] = "시중가격";
                data[0, 6] = "옵션명1";
                data[0, 7] = "옵션항목1";
                data[0, 8] = "옵션명2";
                data[0, 9] = "옵션항목2";
                data[0, 10] = "옵션명3";
                data[0, 11] = "옵션항목3";
                data[0, 12] = "상품이미지";
                data[0, 13] = "상품상세설명";
                data[0, 14] = "신상세설명";
                data[0, 15] = "상품재고";
                data[0, 16] = "추가이미지1";
                data[0, 17] = "추가이미지2";
                data[0, 18] = "추가이미지3";
                data[0, 19] = "추가이미지4";
                data[0, 20] = "상품 URL";


                int i = 0;
                
                foreach (ViewModel.MyDBViewModel row in myDBView_list)
                {
                    ++i;
                    data[i, 0] = row.Num;
                    data[i, 1] = row.Id;
                    data[i, 2] = row.Prd_name;
                    data[i, 3] = row.Prd_price;
                    //data[i, 4] = (float.Parse(row.Prd_price) * float.Parse(txtExChange.Text)).ToString();
                    data[i, 4] = row.Prd_price;
                    data[i, 5] = row.Prd_promo;
                    data[i, 6] = row.Opt_1;
                    data[i, 7] = row.Opt_val_1;
                    data[i, 8] = row.Opt_2;
                    data[i, 9] = row.Opt_val_2;
                    data[i, 10] = row.Opt_3;
                    data[i, 11] = row.Opt_val_3;
                    data[i, 12] = row.Prd_img.ToString();
                    data[i, 13] = row.Detail_img;
                    data[i, 14] = row.Detail_img;
                    data[i, 15] = row.Prd_stock;
                    data[i, 16] = row.Add_img_1;
                    data[i, 17] = row.Add_img_2;
                    data[i, 18] = row.Add_img_3;
                    data[i, 19] = row.Add_img_4;
                    data[i, 20] = "https://detail.tmall.com/item.htm?id=" + row.Id;
                }
                excelSheet.Range[excelSheet.Cells[1, 1], excelSheet.Cells[r, c]].Value2 = data;
                excel.Visible = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "generateExcel");
            }
            finally
            {
                Marshal.ReleaseComObject(excelSheet);
                Marshal.ReleaseComObject(excelWorkBook);
                Marshal.ReleaseComObject(excel);
            }
        }

        #endregion
        // 수집결과 출력
        private void fnGenResult()
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
        // 시스템이 종료될 때 puppeteer도 함께 종료시킴.
        private async void Window_Closed(object sender, EventArgs e)
        {
            if (!puppeteer_page.IsClosed) await puppeteer_page.CloseAsync();
            if (!browser.IsDisposed) browser.Dispose();
        }
    }
}
