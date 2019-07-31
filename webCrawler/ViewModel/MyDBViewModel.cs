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
        private string _prd_category;          // 상품 카테고리
        private string _prd_name;                // 상품명
        private string _prd_attr;                   // 상품 속성
        private string _detail_yn;                 // 상세정보 입력
        private string _prd_price;                 // 공급가격
        //private string _prd_price_won;         // 판매가격
        private string _prd_promo;               // 시중가격
        //private string _prd_opt;                   // 상품 옵션
        private string _opt_1;
        private string _opt_val_1;
        private string _opt_2;
        private string _opt_val_2;
        private string _opt_3;
        private string _opt_val_3;
        private string _opt_imgs;                 // 옵션 이미지 url ^^ 옵션명 리스트
        private string _prd_stock;                // 상품 재고
        private string _detail_img;               // 상품 상세 이미지
        private string _add_img_1;              // 추가이미지1
        private string _add_img_2;              // 추가이미지2
        private string _add_img_3;              // 추가이미지3
        private string _add_img_4;              // 추가이미지4
        private string _created_date;          // 생성일
        private string _updated_date;          // 수정일
        private string _user_id;                    // 사용자 ID
        public MyDBViewModel() { }
        public MyDBViewModel(bool isSelected, int num, string id, BitmapImage prd_img, string prd_category, string prd_name,
            string prd_attr, string detail_yn, string prd_price, string prd_promo,
            string opt_1, string opt_val_1, string opt_2, string opt_val_2, string opt_3, string opt_val_3, string opt_imgs, 
            string prd_stock, string detail_img, string add_img_1, string add_img_2, string add_img_3, string add_img_4, 
            string created_date, string updated_date, string user_id)
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
            //_prd_price_won = prd_price_won;
            _prd_promo = prd_promo;
            _opt_1 = opt_1;
            _opt_val_1 = opt_val_1;
            _opt_2 = opt_2;
            _opt_val_2 = opt_val_2;
            _opt_3 = opt_3;
            _opt_val_3 = opt_val_3;
            _opt_imgs = opt_imgs;
            _prd_stock = prd_stock;
            _detail_img = detail_img;
            _add_img_1 = add_img_1;
            _add_img_2 = add_img_2;
            _add_img_3 = add_img_3;
            _add_img_4 = add_img_4;
            _created_date = created_date;
            _updated_date = updated_date;
            _user_id = user_id;
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
        //public string Prd_price_won
        //{
        //    get { return _prd_price_won; }
        //    set { _prd_price_won = value; }
        //}
        public string Prd_promo
        {
            get { return _prd_promo; }
            set { _prd_promo = value; }
        }
        public string Opt_1
        {
            get { return _opt_1; }
            set { _opt_1 = value; }
        }
        public string Opt_val_1
        {
            get { return _opt_val_1; }
            set { _opt_val_1 = value; }
        }
        public string Opt_2
        {
            get { return _opt_2; }
            set { _opt_2 = value; }
        }
        public string Opt_val_2
        {
            get { return _opt_val_2; }
            set { _opt_val_2 = value; }
        }
        public string Opt_3
        {
            get { return _opt_3; }
            set { _opt_3 = value; }
        }
        public string Opt_val_3
        {
            get { return _opt_val_3; }
            set { _opt_val_3 = value; }
        }
        public string Opt_imgs
        {
            get { return _opt_imgs; }
            set { _opt_imgs = value; }
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
        public string Add_img_1
        {
            get { return _add_img_1; }
            set { _add_img_1 = value; }
        }
        public string Add_img_2
        {
            get { return _add_img_2; }
            set { _add_img_2 = value; }
        }
        public string Add_img_3
        {
            get { return _add_img_3; }
            set { _add_img_3 = value; }
        }
        public string Add_img_4
        {
            get { return _add_img_4; }
            set { _add_img_4 = value; }
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
        public string User_id
        {
            get { return _user_id; }
            set { _user_id = value; }
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
