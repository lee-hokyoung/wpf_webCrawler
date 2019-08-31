using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace webCrawler.ViewModel
{
    public class CategoryViewModel_L : ViewModelBase
    {
        private int _num;
        private int _id;
        private string _cate_name;
        private string _cate_type;
        private string _L;
        private string _CODE;
        private CategoryViewModel_L() { }

        public CategoryViewModel_L(int num, int id, string cate_name, string cate_type, string L, string CODE)
        {
            _num = num;
            _id = id;
            _cate_name = cate_name;
            _cate_type = cate_type;
            _L = L;
            _CODE = CODE;
        }
        public int Num
        {
            get { return _num; }
            set { _num = value; }
        }
        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }
        public string Cate_name
        {
            get { return _cate_name; }
            set { _cate_name = value; }
        }
        public string Cate_type
        {
            get { return _cate_type; }
            set { _cate_type = value; }
        }
        public string L
        {
            get { return _L; }
            set { _L = value; }
        }
        public string CODE
        {
            get { return _CODE; }
            set { _CODE = value; }
        }
    }
}
