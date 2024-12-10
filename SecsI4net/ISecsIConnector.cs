using Secs4Net;

namespace SecsI4net
{
    public interface ISecsIConnector : IDisposable
    {
        void SendAsync(SecsMessage message);

        void SendAsync(byte[] message);
    }
}