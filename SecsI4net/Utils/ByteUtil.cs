using System;
using System.Collections.Generic;
using System.Text;

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
    }
}
