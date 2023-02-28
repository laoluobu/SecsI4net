using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecsI4net.Enums
{
    internal interface SECSIHandshake
    {
        /// <summary>
        /// Request to Send
        /// </summary>
        static byte[] ENQ = new byte[] { 5 };
        //Ready to Receive
        static byte[] EOT = new byte[] { 4 };
        //Correct Reception
        static byte[] ACK = new byte[] { 6 };
        /// <summary>
        /// Incorrect Reception
        /// </summary>
        static byte[] NAK = new byte[] { 21 };
    }
}
