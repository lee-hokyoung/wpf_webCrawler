using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace webCrawler.Contoller
{
    public static class Category
    {
        public static List<ViewModel.CategoryViewModel> category_list = new List<ViewModel.CategoryViewModel>();
        public static void createCategory(string txtCategoryName, string txtCategoryDesc)
        {
            MySqlConnection conn = new MySqlConnection(Properties.Settings.Default.strConn);
            try
            {
                conn.Open();
                string isExist = string.Format("SELECT count(*) as cnt FROM taobao_category WHERE cate_name = '{0}'; ", txtCategoryName);
                MySqlCommand exCmd = new MySqlCommand(isExist, conn);
                int count = Convert.ToInt32(exCmd.ExecuteScalar());
                if (count > 0)
                {
                    MessageBox.Show("동일한 이름의 카테고리가 이미 존재합니다..");
                }
                else
                {
                    string query = string.Format("INSERT INTO taobao_category (cate_name, cate_desc) VALUES ('{0}', '{1}')", txtCategoryName, txtCategoryDesc);
                    List<string> insert_rows = new List<string>();

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Connection = conn;
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandTimeout = 1000;
                        cmd.ExecuteNonQuery();
                    }
                    MessageBox.Show("등록되었습니다.");
                }
            }
            catch(Exception e)
            {
                MessageBox.Show(e.ToString());
            }
            finally
            {
                if (conn != null) conn.Close();
            }
        }
        public static List<ViewModel.CategoryViewModel> readCategory()
        {
            MySqlConnection conn = null;
            category_list = new List<ViewModel.CategoryViewModel>();
            int num = 0;
            try
            {
                conn = new MySqlConnection(Properties.Settings.Default.strConn);
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conn;
                conn.Open();
                cmd.Prepare();
                cmd.CommandText = "SELECT * FROM taobao_category; ";
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    category_list.Add(new ViewModel.CategoryViewModel(
                        false, ++num, int.Parse(reader["id"].ToString()), reader["cate_name"].ToString(), reader["cate_desc"].ToString()
                        )
                    );
                }
            }
            catch(Exception e)
            {
                MessageBox.Show(e.ToString());
            }
            finally
            {
                if (conn != null) conn.Close();
            }
            return category_list;
        }
        public static void updateCategory(string cate_id, string cate_name, string cate_desc)
        {
            MySqlConnection conn = null;
            try
            {
                conn = new MySqlConnection(Properties.Settings.Default.strConn);
                conn.Open();
                string query  = string.Format("UPDATE taobao_category SET cate_name = '{0}', cate_desc = '{1}' WHERE id = '{2}' ; ", cate_name, cate_desc, cate_id);
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandTimeout = 1000;
                    cmd.ExecuteNonQuery();
                }
                MessageBox.Show("수정되었습니다.");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
            finally
            {
                if (conn != null) conn.Close();
            }
        }
        public static void deleteCategory(string cate_id)
        {
            MySqlConnection conn = null;
            try
            {
                conn = new MySqlConnection(Properties.Settings.Default.strConn);
                conn.Open();
                string query = string.Format("DELETE FROM taobao_category WHERE id = '{0}' ; ", cate_id);
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandTimeout = 1000;
                    cmd.ExecuteNonQuery();
                }
                MessageBox.Show("삭제되었습니다.");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
            finally
            {
                if(conn != null) conn.Close();
            }
        }
    }
}
