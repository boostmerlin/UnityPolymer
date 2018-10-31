//#define DEBUG_NET

using System;
using System.Net.Sockets;
using System.Net;
using System.Text;

namespace Ginkgo.Net
{
    /// <summary>
    /// 处理socket连接，支持同步方式和异步方式。
    /// </summary>
#if false
    public class TcpConnection
    {
        public const int DEFAULT_MAX_MSG_LEN = 10 * 1024;

        private Socket mSocket;
        protected CirculeBuffer mReceiveBuffer;
        protected CirculeBuffer mSendBuffer;

        private byte[] mTempBuffer;

        public SocketMessageHandler onStateReport;
        public SocketStateHandler onConnect;

        //只留供多线程版本使用。
        private ManualResetEvent mResetEvent = new ManualResetEvent(false);
        private object syncObject = new object();

        private bool m_isConnected;

        public bool IsConnected()
        {
            return m_isConnected;
        }

        public TcpConnection()
        {
            mReceiveBuffer = new CirculeBuffer();
            mSendBuffer = new CirculeBuffer();
            mTempBuffer = new byte[DEFAULT_MAX_MSG_LEN];
        }

        public CirculeBuffer ReceiveBuffer { get { return mReceiveBuffer; } }

        public CirculeBuffer SendBuffer { get { return mSendBuffer; } }

        /// <summary>
        /// 为 Socket 设置低级操作模式
        /// </summary>
        private void SetIOControl()
        {
            byte[] inValue = new byte[] { 1, 0, 0, 0, 0x88, 0x13, 0, 0, 0xd0, 0x07, 0, 0 };// 首次探测时间5 秒, 间隔侦测时间2 秒
            mSocket.IOControl(IOControlCode.KeepAliveValues, inValue, null);
        }

        private void BeginReceive()
        {
            //int bytes = mSocket.IOControl(IOControlCode)
            if (mSocket.Available > 0)
            {
                mSocket.BeginReceive(mTempBuffer, 0, mTempBuffer.Length, SocketFlags.None, new AsyncCallback(OnReceiveCallback), mSocket);
            }
        }
        /// <summary>
        /// 连接后的回调操作
        /// </summary>
        /// <param name="ar"></param>
        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                mSocket = ar.AsyncState as Socket;
                mSocket.EndConnect(ar);
                if (mSocket.Connected)
                {
                   // StartKeepAlive();
                    m_isConnected = true;
                    if (onConnect != null)
                    {
                        onConnect(SocketError.Success, true);
                    }
                }
            }
            catch (SocketException e)
            {
                if (onStateReport != null)
                {
                    onStateReport(e.Message);
                }
                if (onConnect != null)
                {
                    onConnect(e.SocketErrorCode, mSocket.Connected);
                }
            }
        }

        /// <summary>
        /// 开始一个异步连接
        /// </summary>
        /// <param name="iporhost"></param>
        /// <param name="port"></param>
        public void BeginConnect(string iporhost, int port)
        {
            try
            {
                IPAddress ip = null;
                if (!IPAddress.TryParse(iporhost, out ip))
                {
                    IPHostEntry entry = Dns.GetHostEntry(iporhost);
                    if (entry.AddressList.Length > 0)
                    {
                        ip = entry.AddressList[0];
                    }

                    if (ip == null && onStateReport != null)
                    {
                        onStateReport("IP Address error.");
                        m_isConnected = false;

                        return;
                    }
                }

                if (onStateReport != null)
                {
                    onStateReport("Begin connect in tcp connection.");
                }

                mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                this.SetIOControl();

                mSocket.BeginConnect(ip, port, ConnectCallback, mSocket);

            }
            catch (SocketException e)
            {
                if (onStateReport != null)
                {
                    onStateReport(e.Message);
                }
                if (onConnect != null)
                {
                    onConnect(e.SocketErrorCode, mSocket.Connected);
                }
            }
        }

        /// <summary>
        /// 作为异步连接的回调
        /// </summary>
        /// <param name="ar"></param>
        private void OnReceiveCallback(IAsyncResult ar)
        {
            try
            {
                Socket peerSock = (Socket)ar.AsyncState;
                int bytesRead = peerSock.EndReceive(ar);
                if (bytesRead > 0)
                {
                    mReceiveBuffer.Write(mTempBuffer, 0, bytesRead);
                    Array.Clear(mTempBuffer, 0, mTempBuffer.Length);//
                    mSocket.BeginReceive(mTempBuffer, 0, mTempBuffer.Length, SocketFlags.None, new AsyncCallback(OnReceiveCallback), mSocket);
                }
                else//正赏关闭？
                {
                    if (peerSock.Connected)//上次socket的状态
                    {
                        m_isConnected = false;
                        if (onStateReport != null)
                        {
                            onStateReport("server has shutdown socket!");
                        }

                        if (onConnect != null)
                        {
                            onConnect(SocketError.Shutdown, false);
                            //2-退出，不再执行BeginReceive
                        }
                        return;
                    }
                }
            }
            catch (SocketException e)
            {
                if (onStateReport != null)
                {
                    onStateReport(e.Message);
                }
                if (onConnect != null)
                {
                    onConnect(e.SocketErrorCode, mSocket.Connected);
                }
            }
            catch (Exception ex)
            {
                if (onStateReport != null)
                {
                    onStateReport(ex.ToString());
                }
            }
        }

        /// <summary>
        /// 将缓冲区的数据发出去
        /// </summary>
        public void BeginWrite()
        {
            if (mSocket.Connected && mSendBuffer.Length > 0)
            {
                byte[] data = mSendBuffer.GetBuffer();
                mSocket.BeginSend(data, 0, mSendBuffer.Length, SocketFlags.None, SendCallback, mSocket);

                mSendBuffer.Reset();//reset here?
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                mSocket.EndSend(ar);
            }
            catch (SocketException e)
            {
                if (onStateReport != null)
                {
                    onStateReport(e.Message);
                }
                if (onConnect != null)
                {
                    onConnect(e.SocketErrorCode, mSocket.Connected);
                }
            }
            catch (Exception e)
            {
                if (onStateReport != null)
                {
                    onStateReport(e.Message);
                }
            }
        }

        public void Disconnect()
        {
            //关闭socket
            mSocket.Shutdown(SocketShutdown.Both);
            mSocket.Disconnect(true);
            m_isConnected = false;
            mSocket.Close();
        }

        public bool Update()
        {
            if (null == mSocket || !mSocket.Connected
                || !m_isConnected)
            {
                return false;
            }

            if ((mSocket.Poll(0, SelectMode.SelectRead) && (mSocket.Available == 0)))
            {
                //HACK：这里算异常否，是否需要这里处理
                return false;
            }

            BeginWrite();
            BeginReceive();

            return mSocket.Connected;
        }
    }

#else
    /// <summary>
    /// 使用TcpClient 实现。还会重写一个由socket实现的版本，提供更多控制。。
    /// </summary>

    public class TcpConnection : IDisposable, INetConnection
    {
        public const int DEFAULT_MAX_MSG_LEN = 10 * 1024;
        //wrap 只考虑主机，端口，数据传输
        private TcpClient mTcp;
        public TcpClient ClientTcp { get { return mTcp; } }

        protected CirculeBuffer mReceiveBuffer;
        protected CirculeBuffer mSendBuffer;

        private byte[] mTempBufferRecv;

        private byte[] mTempBufferSend;

        private byte[] mMsgLenBuffer;

        SocketMessageHandler onStateReport;
        SocketStateHandler onConnect;
        NetStateHandler onNetState;
        //    private AutoResetEvent waitHandle = new AutoResetEvent(false);

        public TcpConnection()
        {
            mReceiveBuffer = new CirculeBuffer();
            mSendBuffer = new CirculeBuffer();
            mTempBufferRecv = new byte[DEFAULT_MAX_MSG_LEN];

            mTempBufferSend = new byte[DEFAULT_MAX_MSG_LEN];
            mMsgLenBuffer = new byte[4];
        }

        public CirculeBuffer ReceiveBuffer
        {
            get
            {
                return mReceiveBuffer;
            }
        }

        public CirculeBuffer SendBuffer
        {
            get
            {
                return mSendBuffer;
            }
        }

        public event SocketMessageHandler OnStateReport
        {
            add
            {
                onStateReport += value;
            }
            remove
            {
                onStateReport -= value;
            }
        }

        public event SocketStateHandler OnConnect
        {
            add
            {
                onConnect += value;
            }
            remove
            {
                onConnect -= value;
            }
        }

        //TODO: 支持多线程
        public bool Update()
        {
            if (null == mTcp || !mTcp.Connected)
            {
                return false;
            }

            Socket socket = ClientTcp.Client;
            try
            {
                if (socket.Poll(0, SelectMode.SelectRead))
                {
                    //byte[] buff = new byte[1];
                    //if (socket.Receive(buff, SocketFlags.Peek) == 0)//do more test, sometimes, 
                    //{
                    //    if (OnErrorReport != null)
                    //    {
                    //        OnErrorReport("detect shut down...");
                    //    }

                    //    return true;
                    //}
                }
                if (socket.Poll(0, SelectMode.SelectError))
                {
                    if (onStateReport != null)
                    {
                        onStateReport("Poll Socket error...");
                    }
                    if (onConnect != null)
                    {
                        onConnect(SocketError.SocketError, false);
                    }
                    if (onNetState != null)
                    {
                        onNetState(NetState.Failed);
                    }
                }
            }
            catch (SocketException e)
            {
                if (onStateReport != null)
                {
                    onStateReport("Socket error..." + e.SocketErrorCode);
                }

                //10035 == WSAEWOULDBLOCK
                if (e.NativeErrorCode.Equals(10035))
                {
                    //仍然处于连接状态,但是发送可能被阻塞
                }
                else
                {
                    if (onConnect != null)
                    {
                        //正常关闭?
                        onConnect(e.SocketErrorCode, false);
                        if (onNetState != null)
                        {
                            onNetState(NetState.ShutDown);
                        }
                    }

                    return false;
                }
            }

            BeginWrite();
            BeginReceive();

            return mTcp.Connected;
        }

        public bool IsConnected()
        {
            return mTcp != null && mTcp.Connected;
        }

        public void Disconnect()
        {
            if (mTcp == null)
            {
                return;
            }

            if (mTcp.Connected)
            {
                try
                {
                    //LingerOption lo = new LingerOption(true, 1);
                    //     mTcp.LingerState = lo;
                    mTcp.GetStream().Close();
                    mTcp.Client.Close();
                    mTcp.Close();
                    mTcp = null;
                }
                catch (Exception e)
                {
                    if (onStateReport != null)
                    {
                        onStateReport(e.Message);
                    }

                }
                if (onStateReport != null)
                {
                    onStateReport("Connection disconnected manually by client.");
                }
                if (onNetState != null)
                {
                    onNetState(NetState.ShutDown);
                }
            }
        }

        public void BeginConnect(string iporhost, int port)
        {
            if (mTcp != null)
            {
                Disconnect();
            }

            mTcp = new TcpClient();
            try
            {
                IPAddress ip = null;
                if (!IPAddress.TryParse(iporhost, out ip))
                {
                    IPHostEntry entry = Dns.GetHostEntry(iporhost);
                    if (entry.AddressList.Length > 0)
                    {
                        ip = entry.AddressList[0];
                    }
                }

                if (onStateReport != null)
                {
                    onStateReport(string.Format("Begin connect in tcp connection on {0}:{1}", iporhost, ip));
                }

                mTcp.BeginConnect(ip, port, new AsyncCallback(ConnectCallback), mTcp);
                if (ip == null && onStateReport != null)
                {
                    onStateReport("IP Address error.");
                }
            }
            catch (SocketException e)
            {
                if (onStateReport != null)
                {
                    onStateReport(e.Message);
                }
                if (onConnect != null)
                {
                    onConnect(e.SocketErrorCode, mTcp.Connected);
                }
                if (onNetState != null)
                {
                    onNetState(e.SocketErrorCode == SocketError.TimedOut ? NetState.TimedOut : NetState.Failed);
                }
            }
        }

        public void OnFault()
        {
            //Connected = true;
            if (onConnect != null)
            {
                onConnect(SocketError.Fault, true);
            }
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                mTcp.EndConnect(ar);
                if (mTcp.Connected)
                {
                    //Connected = true;
                    if (onConnect != null)
                    {
                        onConnect(SocketError.Success, true);
                    }
                    if(onNetState != null)
                    {
                        onNetState(NetState.Success);
                    }
                }
            }
            catch (SocketException e)
            {
                if (onStateReport != null)
                {
                    onStateReport(e.Message);
                }
                if (onConnect != null)
                {
                    onConnect(e.SocketErrorCode, mTcp.Connected);
                }
                if (onNetState != null)
                {
                    onNetState(e.SocketErrorCode == SocketError.TimedOut ? NetState.TimedOut : NetState.Failed);
                }
            }
        }

        /// <summary>
        /// 将缓冲区的数据发出去
        /// </summary>
        void BeginWrite()
        {
            int len = mSendBuffer.Length;
            int msglen = 0;
            if (mTcp.Client.Connected && len > 0)
            {
                NetworkStream ns = mTcp.GetStream();
                try
                {
                    if (ns.CanWrite)
                    {
                        if (len < sizeof(Int32))
                        {
#if DEBUG_NET
                          Log.Net.PrintWarning("network! data in buffer is less than 4 bytes.");
#endif
                            return;
                        }

                        Array.Clear(mMsgLenBuffer, 0, 4);
                        if (mSendBuffer.Peek(mMsgLenBuffer, 0, 4) > 0)
                        {
                            msglen = BitConverter.ToInt32(mMsgLenBuffer, 0);
                            msglen = IPAddress.NetworkToHostOrder(msglen);
                            if (msglen > len)
                            {
#if DEBUG_NET
                                Log.Net.PrintWarning("network! data in buffer is not a whole one.");
#endif
                                return;
                            }

                            Array.Clear(mTempBufferSend, 0, mTempBufferSend.Length);
                            mSendBuffer.Read(mTempBufferSend, 0, msglen);
                            ns.BeginWrite(mTempBufferSend, 0, msglen, new AsyncCallback(SendCallback), ns);
                        }

                        //byte[] data = mSendBuffer.GetBuffer();                        //BUG ??????
                        //ns.BeginWrite(data, 0, mSendBuffer.Length, new AsyncCallback(SendCallback), ns);
#if DEBUG_NET
                        Log.Net.Print("network! Write Data to Socket: " + len);
                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < msglen; i++)
                        {
                            sb.AppendFormat("{0:X2} ", mTempBufferSend[i]);
                            if ((i + 1) % 16 == 0)
                            {
                                sb.AppendLine();
                            }
                        }
                        Log.Net.Print(sb.ToString());
#endif
                        // mSendBuffer.Reset();//reset here?
                    }
                }
                catch (SocketException e)
                {
                    if (onStateReport != null)
                    {
                        onStateReport(e.Message);
                    }
                    if (onConnect != null)
                    {
                        onConnect(e.SocketErrorCode, mTcp.Connected);
                    }
                    if (onNetState != null)
                    {
                        onNetState(e.SocketErrorCode == SocketError.TimedOut ? NetState.TimedOut : NetState.Failed);
                    }
                }
                catch (Exception e)
                {
                    if (onStateReport != null)
                    {
                        onStateReport(e.Message);
                    }
                    if (onConnect != null)
                    {
                        onConnect(SocketError.SocketError, mTcp.Connected);
                    }
                    if (onNetState != null)
                    {
                        onNetState(NetState.Failed);
                    }
                }


            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                mTcp.GetStream().EndWrite(ar);
            }
            catch (System.IO.IOException e)
            {
                SocketException se = e.InnerException as SocketException;
                if(se != null)
                {
                    if(onConnect!= null)
                    {
                        onConnect(se.SocketErrorCode, mTcp.Connected);
                    }
                    if (onNetState != null)
                    {
                        onNetState(se.SocketErrorCode == SocketError.TimedOut ? NetState.TimedOut : NetState.Failed);
                    }
                }
                if (onStateReport != null)
                {
                    onStateReport(e.Message);
                }
            }
            catch(ObjectDisposedException e) //NetworkStream 是关闭的。 
            {
                if (onStateReport != null)
                {
                    onStateReport(e.Message);
                }
            }
        }

        void BeginReceive()
        {
            try
            {
                if (mTcp.Connected && mTcp.Available > 0)
                {
                    NetworkStream ns = mTcp.GetStream();
                    if (ns.CanRead)
                    {
                        //  byte[] recvBuffer = mReceiveBuffer.GetBuffer();
                        ns.BeginRead(mTempBufferRecv, 0, mTempBufferRecv.Length, new AsyncCallback(ReceiveCallback), ns);

                        //                        waitHandle.WaitOne();
                        //if (onStateReport != null)
                        //{
                        //    onStateReport("begin receive message!");
                        //}
                    }
                }
            }
            catch (Exception e)
            {
                if (onStateReport != null)
                {
                    onStateReport(e.Message);
                }
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                if (mTcp.Connected)
                {
                    NetworkStream ns = mTcp.GetStream();
                    int bytesRead = ns.EndRead(ar);

                    if (bytesRead > 0)
                    {
                        int write = mReceiveBuffer.Write(mTempBufferRecv, 0, bytesRead);
                        if (onStateReport != null)
                        {
                            onStateReport(string.Format("receive data of size: {0}. Current RecvBuffer data Len: {1}", write, mReceiveBuffer.Length));
                        }
                    }
                }

            }
            catch (SocketException e)
            {
                if (onStateReport != null)
                {
                    onStateReport(e.Message);
                }
                if (onConnect != null)
                {
                    onConnect(e.SocketErrorCode, false);
                }
                if (onNetState != null)
                {
                    onNetState(e.SocketErrorCode == SocketError.TimedOut ? NetState.TimedOut : NetState.Failed);
                }
            }
            catch (Exception e)
            {
                if (onStateReport != null)
                {
                    onStateReport(e.Message);
                }
                if (onConnect != null)
                {
                    onConnect(SocketError.Shutdown, false);
                }
                if (onNetState != null)
                {
                    onNetState(NetState.ShutDown);
                }
            }
        }

        private bool disposed = false;

        public event NetStateHandler OnNetState;

        ~TcpConnection()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                //释放托管资源
                if (disposing)
                {
                    
                }
                Disconnect();
                disposed = true;
            }
        }

        public void Connect(string iporhost, int port)
        {
            BeginConnect(iporhost, port);
        }

        public int Send(byte[] buffer)
        {
            return mSendBuffer.Write(buffer, 0, buffer.Length);
        }

        public CirculeBuffer GetDataBuffer()
        {
            return mReceiveBuffer;
        }

    }
#endif
}
