using CefSharp;
using CefSharp.Wpf;
using HtmlAgilityPack;
using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
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
        public string main_url = "https://world.taobao.com/";
        public string login_url = "https://world.taobao.com/markets/all/login";
        
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new ViewModel.ProductViewModel();
            InitializeChromium();
            //Window_Loaded();
            getDbData();
        }
        private void InitializeChromium()
        {
            main_doc.Visibility = Visibility.Visible;
            myDb_doc.Visibility = Visibility.Collapsed;
            CefSettings settings = new CefSettings();
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
                    if (strHtml.IndexOf("描述加载中") == -1 && html_node != null)
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
                string nid, src, alt, detailYn, isExist;
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

                        src = img[0].Attributes["data-src"].Value;
                        alt = img[0].Attributes["alt"].Value;

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
                            "",         // 상품명, 여기서는 크게 필요 없음.
                            category    // 카테고리
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
            HtmlDocument doc = null, doc_li = null, doc_img = null;
            HtmlNodeCollection attributes = null, img_wrap = null, imgs = null, lis = null, price = null, opts = null, stock = null;
            string[] strAttr = null, strImg = null, strOpt = null;
            string sql_id = "", sql_stock = "", sql_prd_name = "", sql_prd_attr = "", sql_detail_yn = "", sql_prd_price = "", sql_prd_opt = "", sql_detail_img = ""
                , sql_created_date = "", sql_updated_date = "", sql_user_id = "";

            MySqlConnection conn = null;
            try
            {
                conn = new MySqlConnection(strConn);
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conn;
                conn.Open();
                cmd.Prepare();
                cmd.Parameters.Add("@id", MySqlDbType.String);
                cmd.Parameters.Add("@prd_name", MySqlDbType.String);
                cmd.Parameters.Add("@prd_attr", MySqlDbType.String);
                cmd.Parameters.Add("@prd_price", MySqlDbType.String);
                cmd.Parameters.Add("@prd_stock", MySqlDbType.String);
                cmd.Parameters.Add("@prd_opt", MySqlDbType.String);
                cmd.Parameters.Add("@detail_img", MySqlDbType.String);
                cmd.Parameters.Add("@updated_date", MySqlDbType.String);

                foreach (var node in html_node)
                {
                    // 상품 속성 : prd_attr
                    doc = new HtmlDocument();
                    doc.LoadHtml(node);

                    sql_id = doc.DocumentNode.SelectNodes("//div[@id='LineZing']")[0].Attributes["itemid"].Value;

                    attributes = doc.DocumentNode.SelectNodes("//div[@class='attributes']");
                    if (attributes != null)
                    {
                        doc_li = new HtmlDocument();
                        doc_li.LoadHtml(attributes[0].InnerHtml);
                        lis = doc_li.DocumentNode.SelectNodes("//li");
                        strAttr = new string[lis.Count];
                        for (var i = 0; i < lis.Count; i++)
                        {
                            strAttr[i] = lis[i].InnerHtml;
                        }
                        sql_prd_attr = String.Join("&$%", strAttr);
                    }
                    // 상품이미지 : prd_img
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
                                strImg[i] = imgs[i].Attributes["data-ks-lazyload"].Value;
                            }
                            sql_detail_img = String.Join("&$%", strImg);
                        }
                    }

                    // 상품 가격 : prd_price
                    price = doc.DocumentNode.SelectNodes("//div[@class='tm-promo-price']/span");
                    if (price != null) sql_prd_price = price[0].InnerText;

                    // 상품 옵션 : prd_opt
                    opts = doc.DocumentNode.SelectNodes("//ul[contains(@class, 'J_TSaleProp')]/li");
                    if (opts != null)
                    {
                        strOpt = new string[opts.Count];
                        for (var i = 0; i < opts.Count; i++)
                        {
                            strOpt[i] = opts[i].InnerText.Replace("\n", "").Replace("\t", "");
                        }
                        sql_prd_opt = String.Join("&$%", strOpt);
                    }

                    // 상품 재고 : J_SpanStock
                    stock = doc.DocumentNode.SelectNodes("//em[@id='J_EmStock']");
                    if(stock != null) sql_stock = stock[0].InnerHtml;
                    //doc.DocumentNode.SelectSingleNode

                    cmd.CommandText = "UPDATE taobao_goods SET " +
                        "detail_yn = 1, prd_attr = @prd_attr, prd_price = @prd_price, prd_opt = @prd_opt, detail_img = @detail_img, updated_date = @updated_date, " +
                        "prd_stock = @prd_stock " +
                        "WHERE id = @id";
                    cmd.Parameters["@id"].Value = sql_id;
                    cmd.Parameters["@prd_attr"].Value = sql_prd_attr;
                    cmd.Parameters["@prd_price"].Value = sql_prd_price;
                    cmd.Parameters["@prd_opt"].Value = sql_prd_opt;
                    cmd.Parameters["@prd_stock"].Value = sql_stock;
                    cmd.Parameters["@detail_img"].Value = sql_detail_img;
                    cmd.Parameters["@updated_date"].Value = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");

                    cmd.ExecuteNonQuery();
                }
                MessageBox.Show("상품수집 완료");
                if (conn != null) conn.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                throw e;
            }
            finally
            {
                if (conn != null) conn.Close();
            }
        }

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
                cmd.CommandText = "SELECT * FROM taobao_goods";
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
                            reader["user_id"] as string
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
        // 상품리스트 파싱하기 & prd_list 에 저장하기
        ArrayList prd_list = new ArrayList();           // Product 클래스를 담아두는 ArrayList
        List<string> id_list = new List<string>();      // 상품 중복 파싱을 방지하기 위한 상품코드 저장하는 List
        List<ViewModel.ProductViewModel> prdView_list = new List<ViewModel.ProductViewModel>();
        int idx = 1;
        //  DB 저장 버튼 클릭 이벤트
        private void btnStoreDB_Click(object sender, RoutedEventArgs e)
        {
            MySqlConnection conn = null;
            List<string> item = new List<string>();
            try
            {
                conn = new MySqlConnection(strConn);
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conn;
                conn.Open();
                cmd.Prepare();
                cmd.Parameters.Add("@id", MySqlDbType.String);
                cmd.Parameters.Add("@prd_img", MySqlDbType.String);
                cmd.Parameters.Add("@prd_name", MySqlDbType.String);
                cmd.Parameters.Add("@prd_category", MySqlDbType.String);
                cmd.Parameters.Add("@created_date", MySqlDbType.String);
                cmd.Parameters.Add("@updated_date", MySqlDbType.String);

                //IEnumerable list = dgTable.ItemsSource as IEnumerable;
                foreach (ViewModel.ProductViewModel row in prdView_list)
                {
                    cmd.CommandText = "SELECT id FROM taobao_goods WHERE id = @id";
                    cmd.Parameters["@id"].Value = row.Id;
                    MySqlDataReader reader = cmd.ExecuteReader();
                    bool isEmpty = reader.Read();
                    reader.Close();
                    if (!isEmpty)
                    {
                        cmd.CommandText = "INSERT INTO taobao_goods(id, prd_img, prd_name, prd_category, created_date) VALUES(@id, @prd_img, @prd_name, @prd_category, @created_date);";
                        cmd.Parameters["@id"].Value = row.Id;
                        cmd.Parameters["@prd_img"].Value = row.Prd_img;
                        cmd.Parameters["@prd_name"].Value = row.Prd_name;
                        cmd.Parameters["@prd_category"].Value = row.Prd_category;
                        cmd.Parameters["@created_date"].Value = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
                        //cmd.ExecuteNonQuery();
                        row.Prd_type = "[Insert]";
                    }
                    else
                    {
                        cmd.CommandText = "UPDATE taobao_goods SET prd_img = @prd_img, prd_name = @prd_name, prd_category = @prd_category, updated_date = @updated_date WHERE id = @id";
                        cmd.Parameters["@id"].Value = row.Id;
                        cmd.Parameters["@prd_img"].Value = row.Prd_img;
                        cmd.Parameters["@prd_name"].Value = row.Prd_name;
                        cmd.Parameters["@prd_category"].Value = row.Prd_category;
                        cmd.Parameters["@updated_date"].Value = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
                        //cmd.ExecuteNonQuery();
                        row.Prd_type = "[Updated]";
                    }
                    row.Prd_exist = "O";

                    item.Add(row.Id);
                    //getDetail(id);
                }
                MessageBox.Show("DB 저장 완료");
            }
            finally
            {
                if (conn != null) conn.Close();
            }
        }
        // 상품정보 가져오기
        private async void btnGetProductInfo_Click(object sender, RoutedEventArgs e)
        {
            html_node = new List<string>();
            foreach (ViewModel.ProductViewModel prd in prdView_list)
            {
                //if (idx++ == 5) break;
                if (prd.IsSelected)
                {
                    browser.Address = "https://detail.tmall.com/item.htm?id=" + prd.Id;
                    await Task.Delay(int.Parse(txtTimeOut.Text) * 1000);
                }
            }
            // 상품 디테일 파싱하기
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
        }

        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            main_doc.Visibility = Visibility.Collapsed;
            myDb_doc.Visibility = Visibility.Visible;
            getMyDB();
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
                cmd.CommandText = "SELECT * FROM taobao_goods";
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
                        false, num++, reader["id"].ToString(), bi, reader["prd_category"].ToString(), reader["prd_name"].ToString(),
                        reader["prd_attr"].ToString().Replace("&$%", "\r\n").Replace("&nbsp;", " "), reader["detail_yn"].ToString(), 
                        reader["prd_price"].ToString(), reader["prd_opt"].ToString().Replace("&$%", "\r\n").Replace("&nbsp;", " "), 
                        reader["prd_stock"].ToString(), reader["detail_img"].ToString().Replace("&$%", "\r\n").Replace("&nbsp;", " "), 
                        reader["created_date"].ToString(), reader["updated_date"].ToString()
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
        private void ChkMyDb_Checked(object sender, RoutedEventArgs e)
        {
            foreach (ViewModel.MyDBViewModel row in myDBView_list)
            {
                row.IsSelected = true;
            }
        }

        private void ChkMyDb_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (ViewModel.MyDBViewModel row in myDBView_list)
            {
                row.IsSelected = false;
            }
        }

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
                int c = dgMyDB.Columns.Count;
                var data = new object[r, c];

                // 헤더 설정
                data[0, 0] = "순서";
                data[0, 1] = "상품코드";
                data[0, 2] = "대표이미지";
                data[0, 3] = "상품명";
                data[0, 4] = "상품속성";
                data[0, 5] = "상품가격";
                data[0, 6] = "상품옵션";
                data[0, 7] = "상품재고";
                data[0, 8] = "상세이미지";
                data[0, 9] = "생성일";
                data[0, 10] = "수정일";


                int i = 0;
                foreach (ViewModel.MyDBViewModel row in myDBView_list)
                {
                    ++i;
                    data[i, 0] = row.Num;
                    data[i, 1] = row.Id;
                    data[i, 2] = row.Prd_img.UriSource.ToString();
                    data[i, 3] = row.Prd_name;
                    data[i, 4] = row.Prd_attr;
                    data[i, 5] = row.Prd_price;
                    data[i, 6] = row.Prd_opt;
                    data[i, 7] = row.Prd_stock;
                    data[i, 8] = row.Detail_img;
                    data[i, 9] = row.Created_date;
                    data[i, 10] = row.Updated_date;
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

    }
}
