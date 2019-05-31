using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media.Imaging;

namespace webCrawler.ViewModel
{
    public class ProductViewModel : INotifyPropertyChanged
    {
        private bool _isSelected = false;   // 체크박스
        private string _id;                 // 상품코드
        private BitmapImage _prd_img;       // 상품 이미지
        private int _row_idx;               // row idx
        private string _detail_yn;          // 상세정보 입력
        private string _prd_exist;          // 상품 등록 여부
        private string _prd_type;           // 등록 타입 : New or Updated
        private string _prd_name;           // 상품명
        private string _prd_category;       // 카테고리
        public ProductViewModel() { }
        public ProductViewModel(bool isSelected, string id, BitmapImage prd_img, int row_idx, string detail_yn, string prd_exist, string prd_type, string prd_name, string prd_category)
        {
            _isSelected = isSelected;
            _id = id;
            _prd_img = prd_img;
            _row_idx = row_idx;
            _detail_yn = detail_yn;
            _prd_exist = prd_exist;
            _prd_type = prd_type;
            _prd_name = prd_name;
            _prd_category = prd_category;
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
            }
        }
        public BitmapImage Prd_img
        {
            get { return _prd_img; }
            set
            {
                _prd_img = value;
            }
        }
    public int Row_idx
        {
            get { return _row_idx; }
            set
            {
                _row_idx = value;
            }
        }
        public string DetailYN
        {
            get { return _detail_yn; }
            set
            {
                _detail_yn = value;
            }
        }
        public string Prd_exist
        {
            get { return _prd_exist; }
            set
            {
                _prd_exist = value;
            }
        }
        public string Prd_type
        {
            get { return _prd_type; }
            set
            {
                _prd_type = value;
            }
        }
        public string Prd_name
        {
            get { return _prd_name; }
            set { _prd_name = value; }
        }
        public string Prd_category
        {
            get { return _prd_category; }
            set { _prd_category = value; }
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
