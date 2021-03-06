﻿using System;
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
        private string _cate_type;
        private string _L;
        private string _M;
        private string _S;
        private string _XS;
        private string _CODE;
        private CategoryViewModel(){}
        
        public CategoryViewModel(bool isSelected, int num, int id, string cate_name, string cate_type, string L, string M, string S, string XS, string CODE)
        {
            _IsSelected = isSelected;
            _num = num;
            _id = id;
            _cate_name = cate_name;
            _cate_type = cate_type;
            _L = L;
            _M = M;
            _S = S;
            _XS = XS;
            _CODE = CODE;
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
        public string M
        {
            get { return _M; }
            set { _M = value; }
        }
        public string S
        {
            get { return _S; }
            set { _S = value; }
        }
        public string XS
        {
            get { return _XS; }
            set { _XS = value; }
        }
        public string CODE
        {
            get { return _CODE; }
            set { _CODE = value; }
        }
    }
}
