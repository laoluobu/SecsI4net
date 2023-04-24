using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;

namespace SerialPortDevice
{
    public class WinSerialPort : ISerialPort
    {
        private SerialPort? Port;

        private readonly object syncLock = new object();

        public void Connection(string COM, int baudRate, Action<ReadOnlyMemory<byte>> dataRecive, int timeOut = 1000, int ReceiveInteval = 20)
        {
            if (Port != null)
                return;
            Port = new SerialPort(COM);
            if (Port.IsOpen)
            {
                return;
            }
            Port.BaudRate = baudRate;
            Port.Parity = Parity.None;  //奇偶校验位
            Port.StopBits = StopBits.One;
            Port.DataBits = 8;
            Port.Handshake = Handshake.None;
            Port.DataReceived += (o, k) =>
            {
                byte[] bytesData;
                //确保收到完整数据
                lock (syncLock)
                {
                    Thread.Sleep(ReceiveInteval);
                    var size = Port.BytesToRead;
                    if (size < 2)
                    {
                        return;
                    }
                    Debug.WriteLine($"DataReceived({COM}): BytesToRead={size}");
                    bytesData = new byte[size];
                    Port.Read(bytesData, 0, bytesData.Length);
                    if (bytesData.Length < 1)
                    {
                        return;
                    }
                }
                Task.Run(() =>
                {
                    dataRecive.Invoke(new ReadOnlyMemory<byte>(bytesData));
                });
            };
            Port.ReadTimeout = timeOut;
            Port.WriteTimeout = timeOut;
            Port.Open();
        }

        public byte[] Read()
        {
            byte[] bytesData = new byte[Port.BytesToRead];
            //从串口读取数据
            Port.Read(bytesData, 0, bytesData.Length);
            return bytesData;
        }

        public void Write(byte[] data)
        {
            Port?.Write(data, 0, data.Length);
        }

        public void SendAsync(ReadOnlyMemory<byte> buffer)
        {
            System.Runtime.InteropServices.MemoryMarshal.TryGetArray(buffer, out var arr);
            Port?.Write(arr.ToArray(), 0, arr.ToArray().Length);
        }

        public void Dispose()
        {
            Port?.Dispose();
        }

        public ReadOnlyMemory<byte> ReadToROM()
        {
            return new ReadOnlyMemory<byte>(Read());
        }

        public bool IsOpen()
        {
            if (Port == null)
            {
                return false;
            }
            return Port.IsOpen;
        }
    }
}