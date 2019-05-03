using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Data;
using System.Windows;
using System.Windows.Media.Imaging;

namespace webCrawler
{
    /// <summary>
    /// myDB.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class myDB : Window
    {
        public string strConn = Properties.Settings.Default.strConn;
        public myDB()
        {
            InitializeComponent();
            Window_Loaded();
            getMyDb();
        }
        // 파싱할 상품 정보 테이블 생성
        DataTable dt;
        DataRow dr;
        private void Window_Loaded()
        {
            dt = new DataTable("my_db");
            dt.Columns.Add("no", typeof(int));
            dt.Columns.Add("id", typeof(string));
            dt.Columns.Add("prd_img", typeof(BitmapImage));
            dt.Columns.Add("prd_name", typeof(string));
            dt.Columns.Add("prd_attr", typeof(string));
            dt.Columns.Add("detail_yn", typeof(string));
            dt.Columns.Add("prd_price", typeof(string));
            dt.Columns.Add("prd_opt", typeof(string));
            dt.Columns.Add("detail_img", typeof(string));
            dt.Columns.Add("created_date", typeof(string));
            dt.Columns.Add("updated_date", typeof(string));
            dt.Columns.Add("user_id", typeof(string));
            dgMyDB.ItemsSource = dt.DefaultView;
        }
        private void getMyDb()
        {
            MySqlConnection conn = null;
            ArrayList db_list = null;
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
                BitmapImage bi = new BitmapImage();
                int idx = 0;
                while (reader.Read())
                {

                    dr = dt.NewRow();
                    dr["no"] = (++idx);
                    dr["id"] = reader["id"] as string;
                    bi = new BitmapImage();
                    bi.BeginInit();
                    bi.UriSource = new Uri("http:" + reader["prd_img"] as string, UriKind.RelativeOrAbsolute);
                    bi.EndInit();
                    dr["prd_img"] = bi;
                    dr["prd_name"] = reader["prd_name"] as string;
                    
                    dr["prd_attr"] = reader["prd_attr"].ToString().Replace("&$%","\r\n").Replace("&nbsp;", " ");
                    dr["detail_yn"] = reader["detail_yn"] as string;
                    dr["prd_price"] = reader["prd_price"] as string;
                    dr["prd_opt"] = reader["prd_opt"].ToString().Replace("&$%", "\r\n").Replace("&nbsp;", " ");
                    dr["detail_yn"] = reader["detail_img"].ToString().Replace("&$%", "\r\n").Replace("&nbsp;", " ");
                    dr["created_date"] = reader["created_date"] as string;
                    dr["updated_date"] = reader["updated_date"] as string;
                    dr["user_id"] = reader["user_id"] as string;
                    dt.Rows.Add(dr);
                    dgMyDB.ItemsSource = dt.DefaultView;
                }
            }
            finally
            {
                if (conn != null) conn.Close();
            }
        }
        private void BtnDownloadExcel_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}