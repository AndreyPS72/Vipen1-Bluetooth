using Android;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using System.Collections.Generic;

namespace VPenExample.Droid
{
    [Activity(Label = "VPenExample", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            if (Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.M)
                ProcessPermissions();

            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            OxyPlot.Xamarin.Forms.Platform.Android.PlotViewRenderer.Init();
            LoadApplication(new App());
        }

        private static int PERMISSION_REQUEST = 10567;

        private string[] permissions =
              {
            Manifest.Permission.AccessCoarseLocation,
            Manifest.Permission.Bluetooth,
            Manifest.Permission.BluetoothAdmin,
            Manifest.Permission.BluetoothPrivileged
                };

        private void ProcessPermissions()
        {
            var ungranted = new List<string>();
            foreach (var p in permissions)
                if (CheckSelfPermission(p) != Permission.Granted)
                    ungranted.Add(p);

            if (ungranted.Count > 0)
            {
                RequestPermissions(ungranted.ToArray(), PERMISSION_REQUEST);
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            try
            {
                if (requestCode == PERMISSION_REQUEST)
                {
                    BluetoothController.Restart();
                }
            }
            catch { return; }
        }
    }
}