//#define DEBUG_NET

using System;
using System.Net.Sockets;
using System.Net;

namespace ML.Net
{
    /// <summary>
    /// 处理socket连接，支持同步方式和异步方式。
    /// </summary>

    //写在外边方便点
    public delegate void ConnectionHandler(SocketError error, bool IsConn);

    public delegate void StateReportHandler(string message);

#if false
    public class TcpConnection
    {
        public const int DEFAULT_MAX_MSG_LEN = 10 * 1024;

        private Socket mSocket;
        protected CirculeBuffer mReceiveBuffer;
        protected CirculeBuffer mSendBuffer;

        private byte[] mTempBuffer;

        public StateReportHandler OnStateReport;
        public ConnectionHandler OnConnect;

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
                    if (OnConnect != null)
                    {
                        OnConnect(SocketError.Success, true);
                    }
                }
            }
            catch (SocketException e)
            {
                if (OnStateReport != null)
                {
                    OnStateReport(e.Message);
                }
                if (OnConnect != null)
                {
                    OnConnect(e.SocketErrorCode, mSocket.Connected);
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

                    if (ip == null && OnStateReport != null)
                    {
                        OnStateReport("IP Address error.");
                        m_isConnected = false;

                        return;
                    }
                }

                if (OnStateReport != null)
                {
                    OnStateReport("Begin connect in tcp connection.");
                }

                mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                this.SetIOControl();

                mSocket.BeginConnect(ip, port, ConnectCallback, mSocket);

            }
            catch (SocketException e)
            {
                if (OnStateReport != null)
                {
                    OnStateReport(e.Message);
                }
                if (OnConnect != null)
                {
                    OnConnect(e.SocketErrorCode, mSocket.Connected);
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
                        if (OnStateReport != null)
                        {
                            OnStateReport("server has shutdown socket!");
                        }

                        if (OnConnect != null)
                        {
                            OnConnect(SocketError.Shutdown, false);
                            //2-退出，不再执行BeginReceive
                        }
                        return;
                    }
                }
            }
            catch (SocketException e)
            {
                if (OnStateReport != null)
                {
                    OnStateReport(e.Message);
                }
                if (OnConnect != null)
                {
                    OnConnect(e.SocketErrorCode, mSocket.Connected);
                }
            }
            catch (Exception ex)
            {
                if (OnStateReport != null)
                {
                    OnStateReport(ex.ToString());
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
                if (OnStateReport != null)
                {
                    OnStateReport(e.Message);
                }
                if (OnConnect != null)
                {
                    OnConnect(e.SocketErrorCode, mSocket.Connected);
                }
            }
            catch (Exception e)
            {
                if (OnStateReport != null)
                {
                    OnStateReport(e.Message);
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

    public class TcpConnection : IDisposable
    {
        public const int DEFAULT_MAX_MSG_LEN = 10 * 1024;
        //只考虑主机，端口，数据传输
        private TcpClient mTcp;
        public TcpClient ClientTcp { get { return mTcp; } }

        protected CirculeBuffer mReceiveBuffer;
        protected CirculeBuffer mSendBuffer;

        private byte[] mTempBufferRecv;

        private byte[] mTempBufferSend;

        private byte[] mMsgLenBuffer;

        public StateReportHandler OnStateReport;
        public ConnectionHandler OnConnect;

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

        //TODO: 支持多线程
        public bool Update()
        {


            if (null == mTcp || !mTcp.Connected)
            {
                return false;
            }

            System.Net.Sockets.Socket socket = ClientTcp.Client;
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
                if (socket.Poll(0, System.Net.Sockets.SelectMode.SelectError))
                {
                    if (OnStateReport != null)
                    {
                        OnStateReport("Poll Socket error...");
                    }
                    if (OnConnect != null)
                    {
                        OnConnect(SocketError.SocketError, false);
                    }
                }
            }
            catch (System.Net.Sockets.SocketException e)
            {
                if (OnStateReport != null)
                {
                    OnStateReport("Socket error..." + e.SocketErrorCode);
                }

                //10035 == WSAEWOULDBLOCK
                if (e.NativeErrorCode.Equals(10035))
                {
                    //仍然处于连接状态,但是发送可能被阻塞
                }
                else
                {
                    if (OnConnect != null)
                    {
                        //正常关闭?
                        OnConnect(e.SocketErrorCode, false);
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
                    if (OnStateReport != null)
                    {
                        OnStateReport(e.Message);
                    }

                }
                if (OnStateReport != null)
                {
                    OnStateReport("connection disconnected manually...");
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

                if (OnStateReport != null)
                {
                    OnStateReport(string.Format("Begin connect in tcp connection on {0}:{1}", iporhost, ip));
                }

                mTcp.BeginConnect(ip, port, new AsyncCallback(ConnectCallback), mTcp);
                if (ip == null && OnStateReport != null)
                {
                    OnStateReport("IP Address error.");
                }
            }
            catch (SocketException e)
            {
                if (OnStateReport != null)
                {
                    OnStateReport(e.Message);
                }
                if (OnConnect != null)
                {
                    OnConnect(e.SocketErrorCode, mTcp.Connected);
                }
            }
        }

        public void OnFault()
        {
            //Connected = true;
            if (OnConnect != null)
            {
                OnConnect(SocketError.Fault, true);
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
                    if (OnConnect != null)
                    {
                        OnConnect(SocketError.Success, true);
                    }
                }
            }
            catch (SocketException e)
            {
                if (OnStateReport != null)
                {
                    OnStateReport(e.Message);
                }
                if (OnConnect != null)
                {
                    OnConnect(e.SocketErrorCode, mTcp.Connected);
                }
            }
        }

        /// <summary>
        /// 将缓冲区的数据发出去
        /// </summary>
        public void BeginWrite()
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
                        if (len < 4)
                        {
#if _DEBUG
                            Logger.Log("network! data in buffer is less than 4 bytes.");
#endif
                            return;
                        }

                        //byte[] data = new byte[4];
                        Array.Clear(mMsgLenBuffer, 0, 4);
                        if (mSendBuffer.Peek(mMsgLenBuffer, 0, 4) > 0)
                        {
                            msglen = BitConverter.ToInt32(mMsgLenBuffer, 0);
                            msglen = System.Net.IPAddress.NetworkToHostOrder(msglen);
                            if (msglen > len)
                            {
#if _DEBUG
                                Logger.Log("network! data in buffer is not a whole one.");
#endif
                                return;
                            }

                            //  len = msglen;
                            //  data = new byte[len];
                            Array.Clear(mTempBufferSend, 0, mTempBufferSend.Length);
                            mSendBuffer.Read(mTempBufferSend, 0, msglen);
                            ns.BeginWrite(mTempBufferSend, 0, msglen, new AsyncCallback(SendCallback), ns);
                        }

                        //byte[] data = mSendBuffer.GetBuffer();                        //BUG ??????
                        //ns.BeginWrite(data, 0, mSendBuffer.Length, new AsyncCallback(SendCallback), ns);
#if DEBUG_NET
                        Logger.Log("network! Write Data to Socket: " + len);
                        System.Text.StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < msglen; i++)
                        {
                            sb.AppendFormat("{0:X2} ", mTempBufferSend[i]);
                            if ((i + 1) % 16 == 0)
                            {
                                sb.AppendLine();
                            }
                        }
                        Logger.Log(sb.ToString());
#endif
                        // mSendBuffer.Reset();//reset here?
                    }
                }
                catch (SocketException e)
                {
                    if (OnStateReport != null)
                    {
                        OnStateReport(e.Message);
                    }
                    if (OnConnect != null)
                    {
                        OnConnect(e.SocketErrorCode, mTcp.Connected);
                    }
                }
                catch (Exception e)
                {
                    if (OnStateReport != null)
                    {
                        OnStateReport(e.Message);
                    }
                    if (OnConnect != null)
                    {
                        OnConnect(SocketError.SocketError, mTcp.Connected);
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
            catch (Exception e)
            {
                if (OnStateReport != null)
                {
                    OnStateReport(e.Message);
                }
            }
        }

        public void BeginReceive()
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
                        //if (OnStateReport != null)
                        //{
                        //    OnStateReport("begin receive message!");
                        //}
                    }
                }
            }
            catch (Exception e)
            {
                if (OnStateReport != null)
                {
                    OnStateReport(e.Message);
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
                        if (OnStateReport != null)
                        {
                            OnStateReport("receive data of size: " + write + " Current RecvBuffer data Len: " + mReceiveBuffer.Length);
                        }
                    }
                }

            }
            catch (SocketException e)
            {
                if (OnStateReport != null)
                {
                    OnStateReport(e.Message);
                }
                if (OnConnect != null)
                {
                    OnConnect(e.SocketErrorCode, false);
                }
            }
            catch (Exception e)
            {
                if (OnStateReport != null)
                {
                    OnStateReport(e.Message);
                }
                if (OnConnect != null)
                {
                    OnConnect(SocketError.Shutdown, false);
                }
            }
        }

        private bool disposed = false;

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
                mTcp.Close();
                disposed = true;
            }
        }
    }
#endif
}
