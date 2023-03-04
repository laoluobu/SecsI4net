using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;
using System.Threading.Tasks;

namespace SerialPortDevice
{
    public class WinSerialPort : ISerialPort
    {
        private SerialPort Port;

        private bool Connected;

        public void Connection(string COM, int baudRate, Action<byte[]> dataRecive)
        {
            if (Port!=null) return;
            try
            {
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
                    byte[] bytesData = new byte[Port.BytesToRead];
                    Port.Read(bytesData, 0, bytesData.Length);
                    dataRecive.Invoke(bytesData);
                };
                Port.ReadTimeout = 1000;
                Port.WriteTimeout = 1000;
                Port.Open();
                Connected = true;
            }
            catch
            {
                Connected = false;
                throw;
            }
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
            Port.Write(data, 0, data.Length);
        }

        public void SendAsync(ReadOnlyMemory<byte> buffer)
        {
            System.Runtime.InteropServices.MemoryMarshal.TryGetArray(buffer, out var arr);
            Port.Write(arr.ToArray(), 0, arr.ToArray().Length);
        }

        public void Dispose()
        {
           // Port?.Dispose();
        }
    }
}
