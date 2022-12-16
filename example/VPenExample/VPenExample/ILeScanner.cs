using System;

namespace VPenExample
{
    public interface ILeScanner : IDisposable
    {
        void Start();

        void Stop();

        bool IsRunning { get; }

        void Restart();

        event BleDataEventHandler NewData;
    }

    public class BleData
    {
        public BleData(byte[] data, string address)
        {
            this.Dara = data;
            this.Address = address;
        }

        public byte[] Dara { get; }
        public string Address { get; }
    }

    public class BleDataEventArgs : EventArgs
    {
        public BleDataEventArgs(BleData data)
        {
            this.Data = data;
        }

        public BleData Data { get; private set; }
    }

    public delegate void BleDataEventHandler(object sender, BleDataEventArgs e);
}