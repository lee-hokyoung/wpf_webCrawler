using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace webCrawler
{
    public class Product
    {
        private string _id;
        private string _src;
        private string _alt;
        private int _row_idx;
        private string _detail_yn;
        public Product()
        {

        }
        public Product(string id, string src, string alt, int row_idx, string detail_yn)
        {
            _id = id;
            _src = src;
            _alt = alt;
            _row_idx = row_idx;
            _detail_yn = detail_yn;
        }
        public string Id
        {
            get { return _id; }
            set { _id = value; }
        }
        public string Src
        {
            get { return _src; }
            set { _src = value; }
        }
        public string Alt
        {
            get { return _alt; }
            set { _alt = value; }
        }
        public int Row_idx
        {
            get { return _row_idx; }
            set { _row_idx = value; }
        }
        public string DetailYN
        {
            get { return _detail_yn; }
            set { _detail_yn = value; }
        }
    }
    public class Prd_Store
    {
        private string _id;
        private string _src;
        private string _alt;
        private string _detail_yn;
        public Prd_Store() { }
        public Prd_Store(string id, string src, string alt, string detail_yn)
        {
            _id = id;
            _src = src;
            _alt = alt;
            _detail_yn = detail_yn;
        }
        public string Id
        {
            get { return _id; }
            set { _id = value; }
        }
        public string Src
        {
            get { return _src; }
            set { _src = value; }
        }
        public string Alt
        {
            get { return _alt; }
            set { _alt = value; }
        }
        public string DetailYN
        {
            get { return _detail_yn; }
            set { _detail_yn = value; }
        }
    }
}
