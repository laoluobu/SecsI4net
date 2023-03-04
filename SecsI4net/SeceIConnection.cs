
using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using Secs4Net;
using SecsI4net.Enums;
using SerialPortDevice;

namespace SecsI4net
{
    public class SeceIConnection : ISecsIConnection
    {
        private ISerialPort Port;

        private bool isReadyToReceive;

        private Action<byte[]> MessageRecive;

        public int T3=3000;

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
                EncodeMessage(message, 10, 0, buffer);
                ReadOnlyMemory<byte> msg=buffer.WrittenMemory;
                ActionSendData(msg);
            }
        }

        private async void ActionSendData(ReadOnlyMemory<byte> msg)
        {
           await Task.Run(() =>
            {
                Port.Write(SECSIHandshake.ENQ);
                int i = 0;
                while (true)
                {
                    if (isReadyToReceive)
                    {
                        isReadyToReceive = false;
                        break;
                    }
                    if (i > T3)
                    {
                        throw new Exception("T3 time out");
                    }
                    i++;
                    Thread.Sleep(1);
                }
                Port.SendAsync(msg);
            });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void EncodeMessage(SecsMessage msg, int id, ushort deviceId, ArrayPoolBufferWriter<byte> buffer)
        {
            buffer.GetSpan(14);
            var length = sizeof(byte);
            buffer.Advance(length);
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
            var lengthBytes = MemoryMarshal.CreateSpan(ref MemoryMarshal.GetReference(buffer.WrittenSpan),1);
#else
        var lengthBytes = new Span<byte>(Unsafe.AsPointer(ref MemoryMarshal.GetReference(buffer.WrittenSpan)),1);
#endif
            lengthBytes[0] = (byte)(buffer.WrittenCount - length);

            getCheksum(buffer);
        }

        public void SendAsync(byte[] message)
        {
            Port.Write(SECSIHandshake.ENQ);
            Port.Write(message);
        }

        private static void getCheksum(ArrayPoolBufferWriter<byte>  data)
        {
            int cks = 0;
            ReadOnlyMemory<byte> s = data.WrittenMemory;
            var d=s.ToArray();
            for(int i=1;i<d.Length;i++)
            {
                cks = (cks + d[i]) % 0xffff;
            }
            data.Write((byte)((cks & 0xff00) >> 8));
            data.Write((byte)(cks & 0xff));
        }
    }
}
