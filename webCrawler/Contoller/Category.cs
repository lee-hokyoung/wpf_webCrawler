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
        public static void createCategory(string txtCategoryName, string txtCategoryDesc, string txtCategoryCode)
        {
            MySqlConnection conn = new MySqlConnection(Properties.Settings.Default.strConn);
            try
            {
                conn.Open();
                int cnt = 0, intOut = 0;
                string[] arrCate = txtCategoryName.Split('/');
                string[] arrCode = txtCategoryCode.Split('/');
                string strL = "", strM = "", strS = "", strXS = "";
                MySqlDataReader reader = null;
                // 카테고리 대분류 입력
                if (arrCate.Length > 0)
                {
                    using (MySqlCommand cmd = new MySqlCommand(string.Format("SELECT count(*) as cnt, L FROM category WHERE cate_type = 'A' AND cate_name = '{0}'; ", arrCate[0]), conn))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandTimeout = 1000;
                        //cmd.Prepare();
                        reader = cmd.ExecuteReader();
                        reader.Read();
                        cnt = Convert.ToInt32(reader["cnt"]);
                        if (reader["L"] != null) strL = reader["L"].ToString();
                    }
                    //  등록된 카테고리 명이 없을 경우 
                    if (cnt == 0)
                    {
                        int max = 0;
                        string strMax = "";
                        StringBuilder insertQuery = new StringBuilder("INSERT INTO category(cate_name, cate_type, L) ");
                        using (MySqlCommand cmd = new MySqlCommand(string.Format("SELECT MAX(L) as L FROM category;"), conn))
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.CommandTimeout = 1000;
                            reader = cmd.ExecuteReader();
                            reader.Read();
                            if (int.TryParse(reader["L"].ToString(), out intOut)) {
                                max = Convert.ToInt32(reader["L"]) + 1;
                                strMax = "00" + max.ToString();
                                strL = strMax.Substring(strMax.Length - 2, 2);
                            }
                            else strL = "00";
                            insertQuery.Append(string.Format("VALUES ('{0}', '{1}', '{2}'); ", arrCate[0], "A", strL));
                        }
                        using (MySqlCommand cmd = new MySqlCommand(insertQuery.ToString(), conn))
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.CommandTimeout = 1000;
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                if (arrCate.Length > 1)
                {
                    using (MySqlCommand cmd = new MySqlCommand(string.Format("SELECT count(*) as cnt, M FROM category WHERE cate_type = 'B' AND cate_name = '{0}' AND L = '{1}'; ", arrCate[1], strL), conn))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandTimeout = 1000;
                        //cmd.Prepare();
                        reader = cmd.ExecuteReader();
                        reader.Read();
                        cnt = Convert.ToInt32(reader["cnt"]);
                        if (reader["M"] != null) strM = reader["M"].ToString();
                    }
                    //  등록된 카테고리 명이 없을 경우 
                    if (cnt == 0)
                    {
                        int max = 0;
                        string strMax = "", code = "00";
                        if (arrCode.Length > 1) code = arrCode[1];
                        StringBuilder insertQuery = new StringBuilder("INSERT INTO category(cate_name, cate_type, L, M) ");
                        using (MySqlCommand cmd = new MySqlCommand(string.Format("SELECT MAX(M) as M FROM category WHERE L = '{0}';", code), conn))
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.CommandTimeout = 1000;
                            reader = cmd.ExecuteReader();
                            reader.Read();
                            if (int.TryParse(reader["M"].ToString(), out intOut))
                            {
                                max = Convert.ToInt32(reader["M"]) + 1;
                                strMax = "00" + max.ToString();
                                strM = strMax.Substring(strMax.Length - 2, 2);
                            }
                            else strM = "00";
                            insertQuery.Append(string.Format("VALUES ('{0}', '{1}', '{2}', '{3}'); ", arrCate[1], "B", strL, strM));
                        }
                        using (MySqlCommand cmd = new MySqlCommand(insertQuery.ToString(), conn))
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.CommandTimeout = 1000;
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                if (arrCate.Length > 2)
                {
                    using (MySqlCommand cmd = new MySqlCommand(string.Format("SELECT count(*) as cnt, S FROM category WHERE cate_type = 'C' AND cate_name = '{0}' AND L = '{1}' AND M = '{2}'; ", arrCate[2], strL, strM), conn))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandTimeout = 1000;
                        //cmd.Prepare();
                        reader = cmd.ExecuteReader();
                        reader.Read();
                        cnt = Convert.ToInt32(reader["cnt"]);
                        if (reader["S"] != null) strS = reader["S"].ToString();
                    }
                    //  등록된 카테고리 명이 없을 경우 
                    if (cnt == 0)
                    {
                        int max = 0;
                        string strMax = "", code = "00";
                        if (arrCode.Length > 2) code = arrCode[2];
                        StringBuilder insertQuery = new StringBuilder("INSERT INTO category(cate_name, cate_type, L, M, S) ");
                        using (MySqlCommand cmd = new MySqlCommand(string.Format("SELECT MAX(S) as S FROM category WHERE M = '{0}';", code), conn))
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.CommandTimeout = 1000;
                            reader = cmd.ExecuteReader();
                            if (reader.Read())
                            {
                                max = Convert.ToInt32(reader["S"]) + 1;
                                strMax = "00" + max.ToString();
                                strS = strMax.Substring(strMax.Length - 2, 2);
                            }
                            else strS = "00";
                            insertQuery.Append(string.Format("VALUES ('{0}', '{1}', '{2}', '{3}', '{4}'); ", arrCate[2], "C", strL, strM, strS));
                        }
                        using (MySqlCommand cmd = new MySqlCommand(insertQuery.ToString(), conn))
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.CommandTimeout = 1000;
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                if (arrCate.Length > 3)
                {
                    using (MySqlCommand cmd = new MySqlCommand(string.Format("SELECT count(*) as cnt, XS FROM category WHERE cate_type = 'D' AND cate_name = '{0}' AND L = '{1}' AND M = '{2}' AND S = '{3}'; ", arrCate[3], strL, strM, strS), conn))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandTimeout = 1000;
                        //cmd.Prepare();
                        reader = cmd.ExecuteReader();
                        reader.Read();
                        cnt = Convert.ToInt32(reader["cnt"]);
                        if (reader["XS"] != null) strXS = reader["XS"].ToString();
                        if (strXS == "") strXS = "00";
                    }
                    //  등록된 카테고리 명이 없을 경우 
                    if (cnt == 0)
                    {
                        int max = 0;
                        string strMax = "", code = "00";
                        if (arrCode.Length > 3) code = arrCode[3];
                        StringBuilder insertQuery = new StringBuilder("INSERT INTO category(cate_name, cate_type, L, M, S, XS) ");
                        using (MySqlCommand cmd = new MySqlCommand(string.Format("SELECT MAX(XS) as XS FROM category WHERE S = '{0}';", code), conn))
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.CommandTimeout = 1000;
                            reader = cmd.ExecuteReader();
                            if (reader.Read())
                            {
                                max = Convert.ToInt32(reader["XS"]) + 1;
                                strMax = "00" + max.ToString();
                                strXS = strMax.Substring(strMax.Length - 2, 2);
                            }
                            else strXS = "00";
                            insertQuery.Append(string.Format("VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}'); ", arrCate[3], "D", strL, strM, strS, strXS));
                        }
                        using (MySqlCommand cmd = new MySqlCommand(insertQuery.ToString(), conn))
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.CommandTimeout = 1000;
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                if (txtCategoryDesc != "")
                {
                    StringBuilder updateQuery = new StringBuilder(string.Format("UPDATE category SET CODE = '{0}' WHERE L = '{1}' ", txtCategoryDesc, strL));
                    if(strM !="") updateQuery.Append(string.Format("AND M = '{0}' ", strM));
                    if (strS != "") updateQuery.Append(string.Format("AND S = '{0}' ", strS));
                    if (strXS != "") updateQuery.Append(string.Format("AND XS = '{0}' ", strXS));
                    updateQuery.Append(";");
                    using (MySqlCommand cmd = new MySqlCommand(updateQuery.ToString(), conn))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandTimeout = 1000;
                        cmd.ExecuteNonQuery();
                    }
                }
                MessageBox.Show("등록성공");
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
        public static List<ViewModel.CategoryViewModel> readCategory(string cate_type, ViewModel.CategoryViewModel selectedIem = null)
        {
            MySqlConnection conn = null;
            category_list = new List<ViewModel.CategoryViewModel>();
            int num = 0;
            try
            {
                StringBuilder query = new StringBuilder(string.Format("SELECT * FROM category WHERE cate_type = '{0}' ", cate_type));
                if (selectedIem != null)
                {
                    //query.Append(string.Format(" AND L = '{0}' AND M = '{1}' AND S = '{2}' AND XS = '{3}' ", selectedIem.L, selectedIem.M, selectedIem.S));
                    switch (selectedIem.Cate_type)
                    {
                        case "A": query.Append(string.Format(" AND L = '{0}' ", selectedIem.L)); break;
                        case "B": query.Append(string.Format(" AND L = '{0}' AND M = '{1}' ", selectedIem.L, selectedIem.M)); break;
                        case "C": query.Append(string.Format(" AND L = '{0}' AND M = '{1}' AND S = '{2}' ", selectedIem.L, selectedIem.M, selectedIem.S)); break;
                        //case "D": query.Append(string.Format(" AND XS = '{0}' ", selectedIem.XS)); break;
                    }
                }
                query.Append(string.Format(";"));
                conn = new MySqlConnection(Properties.Settings.Default.strConn);
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conn;
                conn.Open();
                cmd.Prepare();
                cmd.CommandText = query.ToString();
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    category_list.Add(new ViewModel.CategoryViewModel(
                        false,
                        ++num, 
                        int.Parse(reader["id"].ToString()),
                        reader["cate_name"].ToString(), 
                        reader["cate_type"].ToString(), 
                        reader["L"].ToString(),
                        reader["M"].ToString(),
                        reader["S"].ToString(),
                        reader["XS"].ToString(),
                        reader["CODE"].ToString()
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
                var cateArr = cate_name.Split('/');
                StringBuilder query = new StringBuilder("UPDATE category SET ");
                if(cateArr.Length > 0) query.Append(string.Format("L = '{0}'", cateArr[0]));
                if(cateArr.Length > 1) query.Append(string.Format(", M = '{0}'", cateArr[1]));
                if (cateArr.Length > 2) query.Append(string.Format(", S = '{0}'", cateArr[2]));
                if (cateArr.Length > 3) query.Append(string.Format(", XS = '{0}'", cateArr[3]));
                if (cate_desc != "") query.Append(string.Format(", CODE = '{0}'", cate_desc));
                query.Append(string.Format(" WHERE id = {0}", cate_id));
                conn = new MySqlConnection(Properties.Settings.Default.strConn);
                conn.Open();
                //string query  = string.Format("UPDATE taobao_category SET cate_name = '{0}', cate_desc = '{1}' WHERE id = '{2}' ; ", cate_name, cate_desc, cate_id);
                using (MySqlCommand cmd = new MySqlCommand(query.ToString(), conn))
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
                string query = string.Format("DELETE FROM category WHERE id = '{0}' ; ", cate_id);
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
