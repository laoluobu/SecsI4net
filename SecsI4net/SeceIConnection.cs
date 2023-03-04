using System;
using System.Buffers;
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
        private static int msgNo =0;

        private ISerialPort Port;

        private bool isReadyToReceive;

        private Action<MessageHeader> MessageRecive;

        public int T3=3000;

        public ushort deviceId = 0;

        public event EventHandler<EventArgs> ConnectionLost;
        public SeceIConnection(string COM, Action<MessageHeader> MessageRecive, int baudRate = 9600)
        {
            Port = new WinSerialPort();
            Port.Connection(COM, baudRate, DataRecive);
            this.MessageRecive = MessageRecive;
        }

        private void DataRecive(ReadOnlyMemory<byte> bytes)
        {
            if (bytes.Length == 1)
            {
                switch (bytes.Span[0])
                {
                    case SECSIHandshake.ENQ:
                        Port.Write(new byte[] { SECSIHandshake.EOT });
                        return;
                    case SECSIHandshake.EOT:
                        isReadyToReceive = true;
                        return;
                    case SECSIHandshake.NAK:
                        return;
                    case SECSIHandshake.ACK:
                        return;
                }
            }
            var header = new MessageHeader();

            var messageHaderSeq = bytes.Slice(1, 10);
            var messageHeaderBytes = new byte[10];
            messageHaderSeq.CopyTo(messageHeaderBytes);
            MessageHeader.Decode(messageHeaderBytes, out header);
            MessageRecive.Invoke(header);
            Port.Write(new byte[] { SECSIHandshake.ACK });
        }

        public void Dispose()
        {
            Port?.Dispose();
        }

        public void SendAsync(SecsMessage message)
        {
            using (var buffer = new ArrayPoolBufferWriter<byte>(initialCapacity: 4096))
            {
                EncodeMessage(message, msgNo == int.MaxValue ? 0 : msgNo++, deviceId, buffer);
                ReadOnlyMemory<byte> msg=buffer.WrittenMemory;
                ActionSendData(msg);
            }
        }

        private async void ActionSendData(ReadOnlyMemory<byte> msg)
        {
           await Task.Run(() =>
            {
                Port.Write(new byte[] { SECSIHandshake.ENQ });
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
                       throw new TimeoutException("T3 time out");
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
            Port.Write(new byte[] { SECSIHandshake.ENQ });
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
