using System;
using Xamarin.Forms;

namespace VPenExample
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            this.BindingContext = ViewModel = new MainPageViewModel();
        }

        public MainPageViewModel ViewModel { get; }

        protected override void OnAppearing()
        {
            if (!BluetoothController.LeScanner.IsRunning)
                BluetoothController.LeScanner.Restart();
            BluetoothController.LeScanner.NewData += LeScanner_NewData;
            base.OnAppearing();
        }

        private void LeScanner_NewData(object sender, BleDataEventArgs e)
        {
            Device.BeginInvokeOnMainThread(() => ViewModel.OnNewData(e));
        }

        protected override void OnDisappearing()
        {
            BluetoothController.LeScanner.NewData -= LeScanner_NewData;
            base.OnDisappearing();
        }

        private async void StartAndRead_Clicked(object sender, System.EventArgs e)
        {
            try
            {
                plot.Model = await ViewModel.Start();
            }
            catch (Exception exc)
            {
                await DisplayAlert(this.Title, exc.Message, "Ok");
            }
        }

        private async void Poll_Clicked(object sender, System.EventArgs e)
        {
            try
            {
                await ViewModel.Poll();
            }
            catch (Exception exc)
            {
                await DisplayAlert(this.Title, exc.Message, "Ok");
            }
        }

        private async void Stop_Clicked(object sender, System.EventArgs e)
        {
            try
            {
                ViewModel.StopPoll();
            }
            catch (Exception exc)
            {
                await DisplayAlert(this.Title, exc.Message, "Ok");
            }
        }
    }
}