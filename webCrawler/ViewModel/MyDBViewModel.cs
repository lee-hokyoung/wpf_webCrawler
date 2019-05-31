using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media.Imaging;

namespace webCrawler.ViewModel
{
    public class MyDBViewModel : INotifyPropertyChanged
    {
        private bool _isSelected = false;   // 체크박스
        private int _num;
        private string _id;                 // 상품코드
        private BitmapImage _prd_img;       // 상품 이미지
        private string _prd_category;       // 상품 카테고리
        private string _prd_name;           // 상품명
        private string _prd_attr;           // 상품 속성
        private string _detail_yn;          // 상세정보 입력
        private string _prd_price;          // 상품가격
        private string _prd_opt;            // 상품 옵션
        private string _prd_stock;          // 상품 재고
        private string _detail_img;         // 상품 상세 이미지
        private string _created_date;       // 생성일
        private string _updated_date;       // 수정일
        private string _user_id;            // 사용자 ID
        public MyDBViewModel() { }
        public MyDBViewModel(bool isSelected, int num, string id, BitmapImage prd_img, string prd_category, string prd_name
            , string prd_attr, string detail_yn, string prd_price, string prd_opt, string prd_stock
            , string detail_img, string created_date, string updated_date)
        {
            _isSelected = isSelected;
            _num = num;
            _id = id;
            _prd_img = prd_img;
            _prd_category = prd_category;
            _prd_name = prd_name;
            _prd_attr = prd_attr;
            _detail_yn = detail_yn;
            _prd_price = prd_price;
            _prd_opt = prd_opt;
            _prd_stock = prd_stock;
            _detail_img = detail_img;
            _created_date = created_date;
            _updated_date = updated_date;
            //_user_id = user_id;
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
        public int Num
        {
            get { return _num; }
            set { _num = value; }
        }
        public string Id
        {
            get { return _id; }
            set { _id = value; }
        }
        public BitmapImage Prd_img
        {
            get { return _prd_img; }
            set { _prd_img = value; }
        }
        public string Prd_category
        {
            get { return _prd_category; }
            set { _prd_category = value; }
        }
        public string Prd_name
        {
            get { return _prd_name; }
            set { _prd_name = value; }
        }
        public string Prd_attr
        {
            get { return _prd_attr; }
            set { _prd_attr = value; }
        }
        public string Detail_yn
        {
            get { return _detail_img; }
            set { _detail_img = value; }
        }
        public string Prd_price
        {
            get { return _prd_price; }
            set { _prd_price = value; }
        }
        public string Prd_opt
        {
            get { return _prd_opt; }
            set { _prd_stock = value; }
        }
        public string Prd_stock
        {
            get { return _prd_stock; }
            set { _prd_stock = value; }
        }
        public string Detail_img
        {
            get { return _detail_img; }
            set { _detail_img = value; }
        }
        public string Created_date
        {
            get { return _created_date; }
            set { _created_date = value; }
        }
        public string Updated_date
        {
            get { return _updated_date; }
            set { _updated_date = value; }
        }


        private ObservableCollection<MyDBViewModel> myDBViewCollection = new ObservableCollection<MyDBViewModel>();
        public ObservableCollection<MyDBViewModel> MyDBViewCollection
        {
            get { return myDBViewCollection; }
            set
            {
                myDBViewCollection = value;
                NotifyPropertyChanged("ProductViewCollection");
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }
}
