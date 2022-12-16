using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Threading.Tasks;
using VPenExample.Fft;

namespace VPenExample
{
    public class MainPageViewModel : BaseViewModel
    {
        public void OnNewData(BleDataEventArgs e)
        {
            try
            {
                if (IsPolling) return;

                this.Device = e.Data.Address;
                var data = e.Data.Dara.BytesToStruct<ViPenAdvertising>();
                Velocity = Math.Round(data.Velocity * 0.01, 2);
                Acceleration = Math.Round(data.Acceleration * 0.01, 2);
                Kurtosis = Math.Round(data.Kurtosis * 0.01, 2);
                Temperature = Math.Round(data.Temperature * 0.01, 2);
                LastAdvertisind = DateTime.Now;
            }
            catch { return; }
        }

        private DateTime _lastAdvertisind;
        public DateTime LastAdvertisind { get => _lastAdvertisind; set { _lastAdvertisind = value; OnPropertyChanged(); } }

        private double velocity;
        public double Velocity { get => velocity; set { velocity = value; OnPropertyChanged(); } }
        private double acceleration;
        public double Acceleration { get => acceleration; set { acceleration = value; OnPropertyChanged(); } }
        private double kurtosis;
        public double Kurtosis { get => kurtosis; set { kurtosis = value; OnPropertyChanged(); } }
        private double temperature;
        public double Temperature { get => temperature; set { temperature = value; OnPropertyChanged(); } }
        public string Device { get; set; }

        public OperationToken Token { get; } = new OperationToken();

        public async Task<bool> StartMeasurement(IVPenControl controller)
        {
            try
            {
                this.IsBusy = true;

                if (!controller.IsConnected)
                    await controller.ConnectAsync(Device, Token);
                if (!controller.IsConnected)
                    throw new Exception("GATT connection error!");

                return await controller.StartMeasurementAsync(Device, Token);
            }
            finally
            {
                this.IsBusy = false;
            }
        }

        public async Task<VPenData> GetWaveform(IVPenControl controller)
        {
            try
            {
                this.IsBusy = true;
                return await controller.Download(Device, Token);
            }
            finally
            {
                this.IsBusy = false;
            }
        }

        public async Task<PlotModel> Start()
        {
            if (string.IsNullOrEmpty(Device))
                throw new Exception("Can't find ViPen device!");

            VPenData blocks = null;
            using (var controller = VPenControlManager.GetController())
            {
                try
                {
                    if (await StartMeasurement(controller))
                    {
                        blocks = await GetWaveform(controller);
                    }
                }
                finally
                {
                    await controller.Disconnect();
                }
            }
            var wav = await blocks.CheckAndConvert();

            var model = new PlotModel();

            var yAxis = new LinearAxis();

            yAxis.Position = AxisPosition.Left;
            yAxis.MajorGridlineStyle = LineStyle.Solid;
            yAxis.MinorGridlineStyle = LineStyle.Solid;
            yAxis.MinorGridlineColor = OxyColor.FromArgb(128, 211, 211, 211);

            yAxis.TextColor = OxyColors.Red;
            yAxis.Title = "mm/s";
            yAxis.TickStyle = TickStyle.Inside;

            model.Axes.Add(yAxis);

            var xAxis = new LinearAxis();
            xAxis.MajorGridlineStyle = LineStyle.Solid;
            xAxis.MinorGridlineStyle = LineStyle.Solid;
            xAxis.MinorGridlineColor = OxyColor.FromArgb(128, 211, 211, 211);
            xAxis.Position = AxisPosition.Bottom;
            xAxis.TextColor = OxyColors.Blue;

            xAxis.Title = "Hz";
            xAxis.TickStyle = TickStyle.Inside;

            for (int i = 0; i < wav.Length; i++)
            {
                wav[i] = (float)(wav[i] * (0.55 - 0.46 * Math.Cos((2 * Math.PI * i) / (wav.Length))));
            }

            model.Axes.Add(xAxis);

            var data = new Complex[1024];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = new Complex(wav[i], 0);
            }

            await Task.Run(() => FourierTransform.FFT(data, FourierTransform.Direction.Forward));

            var line = new LineSeries() { Decimator = Decimator.Decimate, StrokeThickness = 2, Color = OxyColors.Red };
            double step = 4000 / (double)(data.Length - 1);
            for (int i = 1; i < 401; i++)
            {
                line.Points.Add(new DataPoint(i * step, data[i].Magnitude * 2 * 1.85));
            }

            model.Series.Add(line);

            return model;
        }

        private bool _isPolling = false;
        public bool IsPolling { get => _isPolling; set { _isPolling = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsNotPolling)); } }

        public bool IsNotPolling => !IsPolling;

        public Action<Exception> ErrorCallback { get; set; }

        public async Task Poll()
        {
            try
            {
                IsPolling = true;

                using (var controller = VPenControlManager.GetController())
                {
                    await controller.ConnectAsync(Device, Token);
                    while (IsPolling)
                    {
                        await controller.Start(Device, Token);

                        var data = await controller.ReadUserData(Device, Token);

                        Velocity = Math.Round(data.Values[0] * 0.01, 2);
                        Acceleration = Math.Round(data.Values[1] * 0.01, 2);
                        Kurtosis = Math.Round(data.Values[2] * 0.01, 2);
                        Temperature = Math.Round(data.Values[3] * 0.01, 2);
                        LastAdvertisind = DateTime.Now;
                    }
                    await controller.Stop(Device, Token);
                    await controller.Disconnect();
                }
            }
            finally
            {
                IsPolling = false;
            }
        }

        public void StopPoll()
        {
            IsPolling = false;
        }
    }
}