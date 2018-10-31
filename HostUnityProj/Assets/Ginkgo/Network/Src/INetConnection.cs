using System.Net.Sockets;

namespace Ginkgo.Net
{
    //自定的网络状态，用于逻辑
    public enum NetState
    {
        //连接成功
        Success = 0,
        //连接或者发送接收失败
        Failed,
        //连接或发送接收超时
        TimedOut,
        //自己或者远程主机关闭
        ShutDown,
    }

    public delegate void NetStateHandler(NetState state);
    public delegate void SocketStateHandler(SocketError error, bool IsConn);
    public delegate void SocketMessageHandler(string message);

    public interface INetConnection
    {
        void Connect(string iporhost, int port);
        bool IsConnected();
        void Disconnect();
        bool Update();
        int Send(byte[] buffer);
        CirculeBuffer GetDataBuffer();

        event SocketMessageHandler OnStateReport;
        event SocketStateHandler OnConnect;
        //TODO:
        event NetStateHandler OnNetState;
    }
}
