using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace VPenExample
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool _isBusy = false;

        public bool IsBusy { get => _isBusy; set { _isBusy = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsReady)); } }

        public bool IsReady => !this._isBusy;
    }
}