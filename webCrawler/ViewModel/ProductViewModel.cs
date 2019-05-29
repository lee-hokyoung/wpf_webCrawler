using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media.Imaging;

namespace webCrawler.ViewModel
{
    public class ProductViewModel : INotifyPropertyChanged
    {
        private bool _isSelected = false;
        private string _id;
        private BitmapImage _prd_img;
        private string _prd_name;
        private int _row_idx;
        private string _detail_yn;
        public ProductViewModel() { }
        public ProductViewModel(bool isSelected, string id, BitmapImage prd_img, string prd_name, int row_idx, string detail_yn)
        {
            _isSelected = isSelected;
            _id = id;
            _prd_img = prd_img;
            _prd_name = prd_name;
            _row_idx = row_idx;
            _detail_yn = detail_yn;
        }
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                NotifyPropertyChanged("IsSelected");
            }
        }
        public string Id
        {
            get { return _id; }
            set
            {
                _id = value;
                NotifyPropertyChanged("Id");
            }
        }
        public BitmapImage Prd_img
        {
            get { return _prd_img; }
            set
            {
                _prd_img = value;
                NotifyPropertyChanged("Prd_img");
            }
        }
        public string Prd_name
        {
            get { return _prd_name; }
            set
            {
                _prd_name = value;
                NotifyPropertyChanged("Prd_name");
            }
        }
        public int Row_idx
        {
            get { return _row_idx; }
            set
            {
                _row_idx = value;
                NotifyPropertyChanged("Row_idx");
            }
        }
        public string DetailYN
        {
            get { return _detail_yn; }
            set
            {
                _detail_yn = value;
                NotifyPropertyChanged("DetailYN");
            }
        }
        private ObservableCollection<ProductViewModel> productViewCollection = new ObservableCollection<ProductViewModel>();
        public ObservableCollection<ProductViewModel> ProductViewCollection
        {
            get { return productViewCollection; }
            set
            {
                productViewCollection = value;
                NotifyPropertyChanged("ProductViewCollection");
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(String info)
        {
            if(PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }
}
