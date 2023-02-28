using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SecsI4net
{
    public interface ISecsIConnection: IDisposable
    {
        void SendAsync(SecsMessage message);
        void SendAsync(byte[] message);
    }
}
