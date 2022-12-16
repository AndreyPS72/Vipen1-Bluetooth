using Xamarin.Forms;

namespace VPenExample
{
    public static class BluetoothController
    {
        static BluetoothController()
        {
            LeScanner = DependencyService.Get<ILeScanner>();
        }

        public static void Restart()
        {
            LeScanner.Restart();
        }

        public static ILeScanner LeScanner { get; }
    }
}