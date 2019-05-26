using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace webCrawler
{
    class DataGridViewModel:ViewModelBase
    {
        ObservableCollection<Product> products;
        ICommand _command;

        public DataGridViewModel()
        {
            ProductInfo = new ObservableCollection<Product>();
        }
        public ObservableCollection<Product> ProductInfo
        {
            get { return products; }
            set
            {
                products = value;
                OnPropertyChanged("ProductInfo");
            }
        }
        public ICommand RemoveCommand
        {
            get
            {
                if (_command == null)
                {
                    _command = new DelegateCommand(CanExecute, Execute);
                }
                return _command;
            }
        }

        private void Execute(object parameter)
        {
            int index = ProductInfo.IndexOf(parameter as Product);
            if (index > -1 && index < ProductInfo.Count)
            {
                ProductInfo.RemoveAt(index);
            }
        }

        private bool CanExecute(object parameter)
        {
            return true;
        }
    }
}
