using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media.Imaging;

namespace webCrawler.ViewModel
{
    public class ResultViewModel
    {
        private string _prd_code;
        private string _exception;
        public ResultViewModel() { }
       public ResultViewModel(string prd_code, string exception)
        {
            _prd_code = prd_code;
            _exception = exception;
        }
        public string Prd_code
        {
            get { return _prd_code; }
            set { _prd_code = value; }
        }
        public string Exception
        {
            get { return _exception; }
            set { _exception = value; }
        }
    }
}