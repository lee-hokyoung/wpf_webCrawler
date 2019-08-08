using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace webCrawler.ViewModel
{
    public class CategoryViewModel:ViewModelBase
    {
        private bool _IsSelected = false;
        private int _num;
        private int _id;
        private string _cate_name;
        private string _cate_desc;
        private CategoryViewModel()
        {

        }
        
        public CategoryViewModel(bool isSelected, int num, int id, string cate_name, string cate_desc)
        {
            _IsSelected = isSelected;
            _num = num;
            _id = id;
            _cate_name = cate_name;
            _cate_desc = cate_desc;
        }
        public bool IsSelected
        {
            get { return _IsSelected; }
            set
            {
                _IsSelected = value;
                OnPropertyChanged("IsSelected");
            }
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
        public string Cate_desc
        {
            get { return _cate_desc; }
            set { _cate_desc = value; }
        }
    }
}
