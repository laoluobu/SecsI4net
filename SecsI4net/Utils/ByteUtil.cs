using System.Text;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;

namespace SecsI4net.Utils
{
    internal static class ByteUtil
    {
        public static string ToHexStr(this byte[] bytes)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(string.Format("{0:X2} ", bytes[i]));
            }
            return builder.ToString().Trim();
        }

        public static byte[] ToBytes(this int value)
        {
            byte[] src = new byte[4];
            src[3] = (byte)(value >> 24 & 0xFF);
            src[2] = (byte)(value >> 16 & 0xFF);
            src[1] = (byte)(value >> 8 & 0xFF);
            src[0] = (byte)(value & 0xFF);
            return src;
        }

        public static void getCheksum(ArrayPoolBufferWriter<byte> data)
        {
            int cks = 0;
            ReadOnlyMemory<byte> s = data.WrittenMemory;
            var d = s.Span;
            for (int i = 1; i < d.Length; i++)
            {
                cks = (cks + d[i]) % 0xffff;
            }
            data.Write((byte)((cks & 0xff00) >> 8));
            data.Write((byte)(cks & 0xff));
        }

        public static byte[] getCheksum(ReadOnlyMemory<byte> data)
        {
            int cks = 0;
            var d = data.Span;
            for (int i = 1; i < d.Length; i++)
            {
                cks = (cks + d[i]) % 0xffff;
            }
            return new byte[] { (byte)((cks & 0xff00) >> 8), (byte)(cks & 0xff) };
        }
    }
}