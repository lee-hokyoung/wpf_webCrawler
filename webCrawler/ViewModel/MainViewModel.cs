using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace webCrawler.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private bool _IsSelected = false;
        private string _id;
        private string _prd_img;
        private string _prd_name;
        private int _row_idx;
        private string _detail_yn;
        public bool IsSelected
        {
            get { return _IsSelected; }
            set
            {
                _IsSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }

        public string Id
        {
            get { return _id; }
            set
            {
                _id = value;
                OnPropertyChanged("Id");
            }
        }
        public string Prd_img
        {
            get { return _prd_img; }
            set
            {
                _prd_img = value;
                OnPropertyChanged("Prd_img");
            }
        }
        public string Prd_name
        {
            get { return _prd_name; }
            set
            {
                _prd_name = value;
                OnPropertyChanged("Prd_name");
            }
        }
        public int Row_idx
        {
            get { return _row_idx; }
            set
            {
                _row_idx = value;
                OnPropertyChanged("Row_idx");
            }
        }
        public string DetailYN
        {
            get { return _detail_yn; }
            set
            {
                _detail_yn = value;
                OnPropertyChanged("DetailYN");
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if(PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
