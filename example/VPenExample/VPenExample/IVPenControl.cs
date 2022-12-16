using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace VPenExample
{
    public class OperationToken : INotifyPropertyChanged
    {
        public bool IsAborted { get; set; } = false;

        private double _progress;

        public double Progress { get => _progress; set { _progress = value; OnPropertyChanged(); } }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct stUser_DataViPen
    {
        public byte Addr;
        public ushort ID;
        public uint Timestamp;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public short[] Values;
    };

    public interface IVPenControl : IDisposable
    {
        Task<bool> ConnectAsync(string connection, OperationToken token);

        Task<bool> StartMeasurementAsync(string connection, OperationToken token);

        Task<VPenData> Download(string connection, OperationToken token);

        Task<stUser_DataViPen> ReadUserData(string connection, OperationToken token);

        Task Start(string connection, OperationToken token);

        Task Stop(string connection, OperationToken token);

        Task Disconnect();

        bool IsConnected { get; }
    }

    public static class VPenControlManager
    {
        public static IVPenControl GetController()
        {
            return DependencyService.Get<IVPenControl>(DependencyFetchTarget.NewInstance);
        }
    }

    public class VPenData
    {
        public stVPenData Header { get; set; }

        public async Task<float[]> CheckAndConvert()
        {
            return await Task.Run(() =>
            {
                var timestamp = Header.Header.Timestamp;
                var factor = Header.Header.Coeff;
                var items = new List<short>();

                foreach (var i in Header.Blocks.OrderBy(j => j.ViPen_Get_Data_Block))
                {
                    if (i.ViPen_Get_Wave_ID != Header.Header.ViPen_Get_Wave_ID)
                        throw new Exception("Incorrect timestamp!");

                    items.AddRange(i.Data);
                }

                var result = new float[items.Count];

                for (int i = 0; i < Math.Min(1600, result.Length); i++)
                {
                    result[i] = items[i] * factor;
                }

                return result;
            });
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct stVPenHeader
    {
        public byte ViPen_Get_Data_Command;
        public byte ViPen_Get_Data_Block;
        public byte ViPen_Get_Wave_ID;

        public byte Reserv1;

        public uint Timestamp;
        public float Coeff;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 138 / 2)]
        private ushort[] Reserv2;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct stVPenData
    {
        public stVPenHeader Header;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 22)]
        public stVPenBlock[] Blocks;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct stVPenBlock
    {
        public byte ViPen_Get_Data_Block;
        public byte ViPen_Get_Wave_ID;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 74)]
        public short[] Data;
    }
}