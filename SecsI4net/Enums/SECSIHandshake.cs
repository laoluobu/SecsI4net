using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecsI4net.Enums
{
    public interface SECSIHandshake
    {
        /// <summary>
        /// Request to Send
        /// </summary>
        const byte ENQ = 5;
        //Ready to Receive
        const byte EOT = 4;
        //Correct Reception
        const byte ACK = 6;
        /// <summary>
        /// Incorrect Reception
        /// </summary>
        const byte NAK = 21;
    }
}
