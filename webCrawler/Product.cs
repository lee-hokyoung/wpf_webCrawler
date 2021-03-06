﻿using System.Windows.Media.Imaging;

namespace webCrawler
{
    public class Product : ViewModelBase
    {
        private bool _IsSelected = false;
        private string _id;
        private BitmapImage _prd_img;
        private string _prd_name;
        private int _row_idx;
        private string _detail_yn;
        public Product()
        {

        }
        public Product(bool IsSelected, string id, BitmapImage prd_img, string prd_name, int row_idx, string detail_yn)
        {
            _IsSelected = IsSelected;
            _id = id;
            _prd_img = prd_img;
            _prd_name = prd_name;
            _row_idx = row_idx;
            _detail_yn = detail_yn;
        }
        public bool IsSelected
        {
            get { return _IsSelected; }
            set
            {
                _IsSelected = value ;
                OnPropertyChanged("IsSelected");
            }
        }

        public string Id
        {
            get { return _id; }
            set {
                _id = value;
                OnPropertyChanged("Id");
            }
        }
        public BitmapImage Prd_img
        {
            get { return _prd_img; }
            set {
                _prd_img = value;
                OnPropertyChanged("Prd_img");
            }
        }
        public string Prd_name
        {
            get { return _prd_name; }
            set {
                _prd_name = value;
                OnPropertyChanged("Prd_name");
            }
        }
        public int Row_idx
        {
            get { return _row_idx; }
            set {
                _row_idx = value;
                OnPropertyChanged("Row_idx");
            }
        }
        public string DetailYN
        {
            get { return _detail_yn; }
            set {
                _detail_yn = value;
                OnPropertyChanged("DetailYN");
            }
        }
    }
    public class Prd_Store
    {
        private string _id;
        private string _prd_img;
        private string _prd_name;
        private string _prd_attr;
        private string _detail_yn;
        private string _prd_price;
        private string _prd_opt;
        private string _detail_img;
        private string _created_date;
        private string _updated_date;
        private string _user_id;
        private string _prd_status;

        public Prd_Store() { }
        public Prd_Store(string id, string prd_img, string prd_attr, string prd_name, string detail_yn, string prd_price, string prd_opt, string detail_img, string created_date, string updated_date, string user_id, string prd_status)
        {
            _id = id;
            _prd_img = prd_img;
            _prd_name = prd_name;
            _prd_attr = prd_attr;
            _detail_yn = detail_yn;
            _prd_price = prd_price;
            _prd_opt = prd_opt;
            _detail_img = detail_img;
            _created_date = created_date;
            _updated_date = updated_date;
            _user_id = user_id;
            _prd_status = prd_status;
        }
        public string Id
        {
            get { return _id; }
            set { _id = value; }
        }
        public string Prd_img
        {
            get { return _prd_img; }
            set { _prd_img = value; }
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
            get { return _detail_yn; }
            set { _detail_yn = value; }
        }
        public string Prd_price
        {
            get { return _prd_name; }
            set { _prd_name = value; }
        }
        public string Prd_opt
        {
            get { return _prd_opt; }
            set { _prd_opt = value; }
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
        public string User_id
        {
            get { return _user_id; }
            set { _user_id = value; }
        }
        public string Prd_status
        {
            get { return _prd_status; }
            set { _prd_status = value; }
        }
    }
}
