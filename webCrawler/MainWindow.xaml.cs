﻿using CefSharp;
using CefSharp.Wpf;
using HtmlAgilityPack;
using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
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
        public string strHtml = "";
        public string detailHtml = "";
        public List<string> html_node = null;
        public MainWindow()
        {
            InitializeComponent();
            InitializeChromium();
            Window_Loaded();
            getDbData();
        }

        private void InitializeChromium()
        {
            CefSettings settings = new CefSettings();
            Cef.Initialize(settings);

            browser = new ChromiumWebBrowser();
            browser.Address = "https://world.taobao.com/markets/all/login";
            Grid.SetRow(browser, 0);

            grid.Children.Add(browser);

            //browser.FrameLoadEnd += WebZBrowserFrameLoadEnded;
            browser.LoadingStateChanged += OnLoadingStateChanged;
        }

        private void OnLoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
        {
            
            if (!e.IsLoading)
            {
                browser.GetSourceAsync().ContinueWith(task =>
                {
                    strHtml = task.Result;
                    if(strHtml.IndexOf("描述加载中") == -1 && html_node != null)
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
                browser.GetSourceAsync().ContinueWith(task =>
                {
                    strHtml = task.Result;
                    browser.ViewSource();
                    if(html_node != null) html_node.Add(strHtml);
                });
            }
        }
        // 파싱할 상품 정보 테이블 생성
        DataTable dt;
        private void Window_Loaded()
        {
            dt = new DataTable("product");
            dt.Columns.Add("no", typeof(int));
            dt.Columns.Add("id", typeof(string));
            dt.Columns.Add("image", typeof(BitmapImage));
            dt.Columns.Add("exist", typeof(string));
            dt.Columns.Add("type", typeof(string));
            dt.Columns.Add("prd_info", typeof(string));

            dt.Columns.Add("src", typeof(string));
            dt.Columns.Add("alt", typeof(string));
            dgTable.ItemsSource = dt.DefaultView;
        }
        // 파싱하기
        private void btnParsor_Click(object sender, RoutedEventArgs e)
        {
            //btnStoreDB.IsEnabled = false;
            //string source = HtmlTextBox.Text;
            //if (source == "") return;
            if (strHtml == "") return;
            getData(strHtml);
        }
        // 상품리스트 파싱하기 & prd_list 에 저장하기
        ArrayList prd_list = new ArrayList();           // Product 클래스를 담아두는 ArrayList
        List<string> id_list = new List<string>();      // 상품 중복 파싱을 방지하기 위한 상품코드 저장하는 List
        DataRow dr;
        int idx = 0;

        private void getData(string source)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(source);
            HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//div[@data-category='auctions']");    // 1. 각각의 상품을 nodes 에 담기
            if (nodes == null) return;

            BitmapImage bi = new BitmapImage();
            HtmlDocument item = new HtmlDocument();
            string nid, src, alt, detailYn;
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
                    dr = dt.NewRow();
                    dr["no"] = (idx + 1);
                    dr["id"] = nid;
                    bi.BeginInit();
                    bi.UriSource = new Uri("http:" + src, UriKind.RelativeOrAbsolute);
                    bi.EndInit();
                    dr["image"] = bi;

                    if (query != null) dr["exist"] = "O";
                    else dr["exist"] = "";
                    dr["type"] = "";

                    if (query == null)
                    {
                        dr["prd_info"] = "";
                        detailYn = "0";
                    }
                    else
                    {
                        if (query.DetailYN == "1")
                        {
                            dr["prd_info"] = "0";
                            detailYn = "1";
                        }
                        else
                        {
                            dr["prd_info"] = "";
                            detailYn = "0";
                        }
                    }
                    dr["src"] = src;
                    dr["alt"] = alt;
                    dt.Rows.Add(dr);
                    dgTable.ItemsSource = dt.DefaultView;
                    prd_list.Add(new Product(nid, src, alt, idx++, detailYn));
                }
            }
            btnStoreDB.IsEnabled = true;
        }

        // 상품등록 여부 확인
        // 1. 해당 아이디의 상품정보를 읽어 온다. category 별로 나누는 것도 고려해 볼 것. 우선은 전체 상품 읽어 오기
        // 2. 불러온 상품 정보는 USER_PRD_LIST 에 담아둔다
        // TODO USER_PRD_LIST 만들것
        ArrayList db_list = null;
        List<string> p_id_list = new List<string>();
        // DB에 저장되어 있는 리스트 불러오기 -> db_list에 저장(Prd_Store 클래스)
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
                    id = reader["p_id"] as string;

                    if (id_list.IndexOf(id) == -1)
                    {
                        p_id_list.Add(id);
                        db_list.Add(new Prd_Store(
                            reader["p_id"] as string, 
                            reader["p_img"] as string, 
                            reader["p_desc"] as string,
                            reader["p_detail_yn"] as string
                            ));
                    }
                }
            }
            finally
            {
                if (conn != null) conn.Close();
            }
        }
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
                cmd.Parameters.Add("@img_src", MySqlDbType.String);
                cmd.Parameters.Add("@alt", MySqlDbType.String);

                //IEnumerable list = dgTable.ItemsSource as IEnumerable;
                foreach (Product row in prd_list)
                {
                    var id = row.Id;
                    var src = row.Src;
                    var alt = row.Alt;
                    cmd.CommandText = "SELECT p_id FROM taobao_goods WHERE p_id = @id";
                    cmd.Parameters["@id"].Value = id;
                    MySqlDataReader reader = cmd.ExecuteReader();
                    bool isEmpty = reader.Read();
                    reader.Close();
                    if (!isEmpty)
                    {
                        cmd.CommandText = "INSERT INTO taobao_goods(p_id, p_img, p_desc) VALUES(@id, @img_src, @alt)";
                        //cmd.Parameters.AddWithValue("@id", id);
                        //cmd.Parameters.AddWithValue("@img_src", src);
                        cmd.Parameters["@id"].Value = id;
                        cmd.Parameters["@img_src"].Value = src;
                        cmd.Parameters["@alt"].Value = alt;
                        cmd.ExecuteNonQuery();
                        (dgTable.Items[row.Row_idx] as DataRowView)[4] = "[New]";
                    }
                    else
                    {
                        cmd.CommandText = "UPDATE taobao_goods SET p_img = @img_src, p_desc = @alt WHERE p_id = @id ";
                        cmd.Parameters["@id"].Value = id;
                        cmd.Parameters["@img_src"].Value = src;
                        cmd.Parameters["@alt"].Value = alt;
                        cmd.ExecuteNonQuery();
                        (dgTable.Items[row.Row_idx] as DataRowView)[4] = "[Updated]";
                    }
                    (dgTable.Items[row.Row_idx] as DataRowView)[3] = "O";

                    item.Add(id);
                    //getDetail(id);
                }
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
            var idx = 0;
            foreach (Product prd in prd_list)
            {
                if (idx++ == 5) break;
                if(prd.DetailYN == "0")
                {
                    browser.Address = "https://detail.tmall.com/item.htm?id=" + prd.Id;
                    await Task.Delay(6000);
                }
            }
            // 상품 디테일 파싱하기
            parsingPrdDetail(html_node);
        }

        private void parsingPrdDetail(List<string> html_node)
        {
            HtmlDocument doc = null, doc_li = null, doc_img = null, doc_option = null;
            HtmlNodeCollection attributes = null, img_wrap = null, imgs = null, lis = null, price = null, opts = null;
            string[] strAttr = null, strImg = null, strOpt = null;
            string sqlAttr = "", sqlImg = "", sqlPrice = "", sqlOpt = "";
            foreach (var node in html_node)
            {
                // 상품 속성
                doc = new HtmlDocument();
                doc.LoadHtml(node);
                attributes = doc.DocumentNode.SelectNodes("//div[@class='attributes']");

                doc_li = new HtmlDocument();
                doc_li.LoadHtml(attributes[0].InnerHtml);
                lis = doc_li.DocumentNode.SelectNodes("//li");
                strAttr = new string[lis.Count];
                for(var i = 0; i < lis.Count; i++)
                {
                    strAttr[i] = lis[i].InnerHtml;
                }
                sqlAttr = String.Join("&$%", strAttr);

                // 상품이미지
                img_wrap = doc.DocumentNode.SelectNodes("//div[contains(@class, 'ke-post')]");
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
                    sqlImg = String.Join("&$%", strImg);
                }

                // 상품 가격
                price = doc.DocumentNode.SelectNodes("//div[@class='tm-promo-price']/span");
                sqlPrice = price[0].InnerText;

                // 상품 옵션
                opts = doc.DocumentNode.SelectNodes("//div[@class='tb-sku']/dl");
                strOpt = new string[opts.Count];
                for(var i = 0; i < opts.Count; i++)
                {
                    strOpt[i] = opts[i].ChildNodes["dt"].InnerText + ":" + opts[i].ChildNodes["dd"].InnerText;
                }
                sqlOpt = String.Join("&$%", strOpt);
            }
        }
    }
}
