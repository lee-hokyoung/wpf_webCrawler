using System.Windows.Media.Imaging;

namespace webCrawler.ViewModel
{
    public class delProductViewModel:ViewModelBase
    {
        private bool _IsSelected = false;
        private int _num;
        private string _id;
        private BitmapImage _prd_img;
        private string _prd_name;
        private delProductViewModel()
        {

        }
        public delProductViewModel(bool isSelected, int num, string id, BitmapImage prd_img, string prd_name)
        {
            _IsSelected = isSelected;
            _num = num;
            _id = id;
            _prd_img = prd_img;
            _prd_name = prd_name;
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
        public string Id
        {
            get { return _id; }
            set{_id = value;}
        }
        public BitmapImage Prd_img
        {
            get { return _prd_img; }
            set{_prd_img = value;}
        }
        public string Prd_name
        {
            get { return _prd_name; }
            set{_prd_name = value;}
        }
    }
}
