using System;
using System.Threading.Tasks;

namespace SerialPortDevice
{
    public interface ISerialPort :IDisposable
    {
        void Connection(string COM,int baudRate, Action<ReadOnlyMemory<byte>> dataRecive);

        void Write(byte[] data);

        void SendAsync(ReadOnlyMemory<byte> buffer);

        byte[] Read();
    }
}
