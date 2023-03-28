using System;

namespace SerialPortDevice
{
    public interface ISerialPort :IDisposable
    {
        void Connection(string COM,int baudRate, Action<ReadOnlyMemory<byte>> dataRecive,int timeOut=1000, int ReceiveInteval = 20);

        void Write(byte[] data);

        void SendAsync(ReadOnlyMemory<byte> buffer);

        byte[] Read();

        ReadOnlyMemory<byte> ReadToROM();

        bool IsOpen();
    }
}
