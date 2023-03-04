using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using CommunityToolkit.HighPerformance.Buffers;
using Secs4Net;
using SecsI4net.Enums;
using SecsI4net.Utils;
using SerialPortDevice;

namespace SecsI4net
{
    public class SeceIConnection : ISecsIConnection
    {
        private static int msgNo =0;

        private ISerialPort Port;

        private bool isReadyToReceive;

        private Action<SecsMessage> MessageRecive;

        public int T3=3000;

        public ushort deviceId = 0;

        public event EventHandler<EventArgs> ConnectionLost;
        public SeceIConnection(string COM, Action<SecsMessage> MessageRecive, int baudRate = 9600)
        {
            Port = new WinSerialPort();
            Port.Connection(COM, baudRate, DataRecive);
            this.MessageRecive = MessageRecive;
        }

        private void DataRecive(ReadOnlyMemory<byte> romByte)
        {
            if (romByte.Length == 1)
            {
                switch (romByte.Span[0])
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
            try
            {
                CheckCKS(romByte);
                var header = EncodeMessageHeader(romByte);
                var item = EncodeItem(romByte);
                var message = AssembleMessagae(header, item);
                MessageRecive.Invoke(message);
                Port.Write(new byte[] { SECSIHandshake.ACK });
            }
            catch(Exception e)
            {
                Trace.WriteLine(e);
            }
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

            ByteUtil.getCheksum(buffer);
        }

        public void SendAsync(byte[] message)
        {
            Port.Write(new byte[] { SECSIHandshake.ENQ });
            Port.Write(message);
        }


        private void CheckCKS(ReadOnlyMemory<byte> data)
        {
            if (data.Length < 13)
            {
                Port.Write(new byte[] { SECSIHandshake.NAK });
                throw new Exception($"Receive bad byte");
            }
            var rigthCheksum = BinaryPrimitives.ReadInt16BigEndian(ByteUtil.getCheksum( data.Slice(1, 10)));
            var mshCheckSum = BinaryPrimitives.ReadInt16BigEndian(data.Slice(data.Length - 2).ToArray());
            if (rigthCheksum != mshCheckSum)
            {
                Port.Write(new byte[] { SECSIHandshake.NAK });
                throw new Exception($"Check Sum Error. Receive {mshCheckSum} instead of {rigthCheksum}");
            }
        }

        private MessageHeader EncodeMessageHeader(ReadOnlyMemory<byte> data)
        {
            var messageHaderSeq = data.Slice(1, 10);
            var header = new MessageHeader();
            var messageHeaderBytes = new byte[10];
            messageHaderSeq.CopyTo(messageHeaderBytes);
            MessageHeader.Decode(messageHeaderBytes, out header);
            return header;  
        }

        private Item? EncodeItem(ReadOnlyMemory<byte> data)
        {
            data = data.Slice(11);
            if (data.Length == 2)
            {
                return null;
            }
            return null;
        }

        private SecsMessage AssembleMessagae(MessageHeader header,Item? item)
        {
            return new SecsMessage(header.S, header.F, header.ReplyExpected)
            {
                SecsItem = item,
            };
        }
    }
}
