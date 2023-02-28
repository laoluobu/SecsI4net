
using CommunityToolkit.HighPerformance.Buffers;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SecsI4net.Enums;
using SecsI4net.Utils;
using SerialPortDevice;
using CommunityToolkit.HighPerformance;

namespace SecsI4net
{
    public class SeceIConnection : ISecsIConnection
    {
        private ISerialPort Port;

        private bool isReadyToReceive;

        private Action<byte[]> MessageRecive;

        public event EventHandler<EventArgs> ConnectionLost;
        public SeceIConnection(string COM, Action<byte[]> MessageRecive, int baudRate = 9600)
        {
            Port = new WinSerialPort();
            Port.Connection(COM, baudRate, DataRecive);
            this.MessageRecive = MessageRecive;
        }

        private void DataRecive(byte[] bytes)
        {
            if (bytes.Length == 1)
            {
                if (bytes[0] == SECSIHandshake.ENQ[0])
                {
                    Port.Write(SECSIHandshake.EOT);
                    return;
                }

                if (bytes[0] == SECSIHandshake.EOT[0])
                {
                    isReadyToReceive = true;
                    return;
                }

                if (bytes[0] == SECSIHandshake.NAK[0])
                {
                    return;
                }


                if (bytes[0] == SECSIHandshake.ACK[0])
                {
                    return;
                }
            }
            
            MessageRecive.Invoke(bytes);
            Port.Write(SECSIHandshake.ACK);
        }

        public void Dispose()
        {
            Port?.Dispose();
        }

        public void SendAsync(SecsMessage message)
        {
            using (var buffer = new ArrayPoolBufferWriter<byte>(initialCapacity: 4096))
            {
                EncodeMessage(message, 1, 1, buffer);
                ReadOnlyMemory<byte> s=buffer.WrittenMemory;
                //Port.Write(SECSIHandshake.ENQ);

                while (isReadyToReceive)
                {
                    isReadyToReceive = false;
                    break;
                }
                Port.SendAsync(s);
            }
           
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void EncodeMessage(SecsMessage msg, int id, ushort deviceId, ArrayPoolBufferWriter<byte> buffer)
        {
            //TODO编码未改
            buffer.GetSpan(14);
            // reserve 4 byte for total length
            buffer.Advance(sizeof(int));
            new MessageHeader
            {
                DeviceId = deviceId,
                ReplyExpected = msg.ReplyExpected,
                S = msg.S,
                F = msg.F,
                MessageType = MessageType.DataMessage,
                Id = id
            }.EncodeTo(buffer);
            msg.SecsItem?.EncodeTo(buffer);

#if NET
            var lengthBytes = MemoryMarshal.CreateSpan(ref MemoryMarshal.GetReference(buffer.WrittenSpan), 4);
#else
        var lengthBytes = new Span<byte>(Unsafe.AsPointer(ref MemoryMarshal.GetReference(buffer.WrittenSpan)), 4);
#endif
            BinaryPrimitives.WriteInt32BigEndian(lengthBytes, buffer.WrittenCount - sizeof(int));
        }

        public void SendAsync(byte[] message)
        {
            Port.Write(message);
        }
    }
}
