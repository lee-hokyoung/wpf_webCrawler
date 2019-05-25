using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using NsExcel = Microsoft.Office.Interop.Excel;

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
                    dr["detail_img"] = reader["detail_img"].ToString().Replace("&$%", "\r\n").Replace("&nbsp;", " ");
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
            generateExcel();
        }
        // 엑셀 생성
        private void generateExcel()
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
                data[0, 0] = "순번";
                data[0, 1] = "상품코드";
                data[0, 2] = "대표이미지";
                data[0, 3] = "상품명";
                data[0, 4] = "판매가격";
                data[0, 5] = "상품속성";
                data[0, 6] = "옵션";
                data[0, 7] = "상세이미지";


                int i = 0;
                foreach (DataRowView row in dgMyDB.ItemsSource)
                {
                    ++i;
                    data[i, 0] = row["no"];
                    data[i, 1] = row["id"];
                    data[i, 2] = row["prd_img"].ToString();
                    data[i, 3] = row["prd_name"];
                    data[i, 4] = row["prd_price"];
                    data[i, 5] = row["prd_attr"];
                    data[i, 6] = row["prd_opt"];
                    data[i, 7] = row["detail_img"];
                }

                excelSheet.Range[excelSheet.Cells[1, 1], excelSheet.Cells[r, c]].Value2 = data;
                excel.Visible = true;

                

                //string strName = "";
                //int intRow = 0, intCol = 0;

                //IEnumerable<Grid> childGrid;
                //childGrid = canvas_realTool2.Children.OfType<Grid>();
                //listSeatInfoMulti = new List<seatInfo>();

                //foreach (Grid child in childGrid)
                //{
                //    foreach (TextBlock txtChild in child.Children)
                //    {
                //        strName = txtChild.Text;
                //        intRow = int.Parse(txtChild.Tag.ToString().Split(',')[0]);
                //        intCol = int.Parse(txtChild.Tag.ToString().Split(',')[1]);
                //        listSeatInfoMulti.Add(new seatInfo { cName = strName, row = intRow, col = intCol });
                //    }
                //}
                //if (isYesNo == MessageBoxResult.Yes)
                //{
                //    var maxRow = from table in 
                //                 orderby table.row descending
                //                 select table.row;
                //    var maxCol = from table in listSeatInfoMulti
                //                 orderby table.col descending
                //                 select table.col;

                //    int y = maxRow.First();
                //    int x = maxCol.First();
                //    //maxRow = maxRow.Max();

                //    var data = new object[y, x];
                //    for (int i = 0; i < listSeatInfoMulti.Count; i++)
                //    {
                //        data[listSeatInfoMulti[i].row - 1, listSeatInfoMulti[i].col - 1] = listSeatInfoMulti[i].cName;
                //    }
                //    var startCell = excelSheet.Cells[1, 1];
                //    var endCell = excelSheet.Cells[y, x];
                //    var writeRange = excelSheet.Range[startCell, endCell];

                //    writeRange.Value2 = data;
                //    excel.Visible = true;
                //}
            }
            catch(Exception ex)
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
    }
}