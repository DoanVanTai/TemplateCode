
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace DVTools
{
    public  class ViewModelBase : INotifyPropertyChanged
    {
        public  event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// Phương thức OnPropertyChanged(“propertyName”): Giúp giao diện WPF tự động cập nhật
        /// khi Property(propertyName) thay đổi giá trị ở behind code.
        /// - propertyName: là tên của Property bị Change value.
        /// - CallerMemberName: Khi propertyName = "" thì lấy “propertyName” là Property gọi phương thức OnPropertyChanged()
        /// </summary>
        /// <param name="propertyName">Name of Property Changed value</param>
        protected virtual  void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    class RelayCommand<T> : ICommand
    {
        private readonly Predicate<T> _canExecute;
        private readonly Action<T> _execute;

        public RelayCommand(Predicate<T> canExecute, Action<T> execute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");
            _canExecute = canExecute;
            _execute = execute;
        }

        public bool CanExecute(object parameter)
        {
            try
            {
                return _canExecute == null ? true : _canExecute((T)parameter);
            }
            catch
            {
                return true;
            }
        }

        public void Execute(object parameter)
        {
            _execute((T)parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}

