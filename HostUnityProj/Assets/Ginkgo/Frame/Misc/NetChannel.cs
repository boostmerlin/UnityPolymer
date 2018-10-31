using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;

using IProtoMessage = Google.Protobuf.IMessage;

namespace Ginkgo.Net
{
    /// <summary>
    /// msg controller manage
    /// </summary>
    public partial class NetChannel
    {
        static Dictionary<Int32, List<BaseMsgService>> m_msgServices = new Dictionary<int, List<BaseMsgService>>();
        //static NetChannel()
        //{

        //}

        public static void RegMsgService(BaseMsgService msgController)
        {
            var processormap = msgController.GetMsgProcessors();
            foreach (var msgid in processormap.Keys)
            {
                RegMsgService(msgid, msgController);
            }
        }

        public static int GetMsgServiceCount(Int32 msgId)
        {
            List<BaseMsgService> list;
            if (m_msgServices.TryGetValue(msgId, out list))
            {
                return list.Count;
            }
            return 0;
        }

        /// <summary>
        /// 因为优先级就是数组顺序，当Priority在[0,GetMsgServiceCount()]之外时
        /// ，优先级和函数调用顺序有关！！
        /// </summary>
        /// <param name="msgId">int</param>
        /// <param name="BaseMsgService">BaseMsgService</param>
        /// <param name="priority">优先级[0, int.max)</param>
        public static void AdjustMsgPriority(Int32 msgId, BaseMsgService msgService, int priority)
        {
            if (priority < 0)
            {
                priority = 0;
            }

            List<BaseMsgService> list;
            if (m_msgServices.TryGetValue(msgId, out list))
            {
                if (priority > list.Count)
                {
                    priority = list.Count;
                }
                if (list.Remove(msgService))
                {
                    list.Insert(priority, msgService);
                }
                else
                {
                    Log.Net.PrintWarning(true, "NetChannel.AdjustMsgPriority, no msgService found: " + msgId);
                }
            }
            else
            {
                Log.Net.PrintWarning(true, "NetChannel.AdjustMsgPriority, no msgid found: " + msgId);
            }
        }
        public static void RegMsgService(Int32 msgId, BaseMsgService msgService)
        {
            List<BaseMsgService> list;
            if (m_msgServices.TryGetValue(msgId, out list))
            {
                if (!list.Contains(msgService))
                {
                    list.Add(msgService);
                }
                else
                {
                    Log.Common.PrintWarning("[RegMsgService] msgid: {0} already register a msgService name: {1}", msgService.FullName);
                }
            }
            else
            {
                list = new List<BaseMsgService>
                {
                    msgService
                };
                m_msgServices.Add(msgId, list);
            }
        }

        public static void UnRegMsgService(BaseMsgService msgService)
        {
            var processormap = msgService.GetMsgProcessors();
            foreach (var msgid in processormap.Keys)
            {
                UnRegMsgService(msgid);
            }
        }

        public static void UnRegMsgService(Int32 msgId)
        {
            m_msgServices.Remove(msgId);
        }

        public static void ProcessMsg(Int32 msgId, IProtoMessage message)
        {
            List<BaseMsgService> list;
            if (m_msgServices.TryGetValue(msgId, out list))
            {
                for (int i = 0; i < list.Count; i++)
                {
                    list[i].CallProcessor(msgId, message);
                }
            }
            else
            {
                Log.Net.PrintWarning("NetChannel.ProcessMsg, no MsgService found for: " + msgId);
            }
        }
    }
    /// <summary>
    /// channel manage manage
    /// </summary>
    public partial class NetChannel
    {
        static int channelId = 0;
        static List<NetChannel> m_channels = new List<NetChannel>();
        static NetChannel currentActiveChannel;

        public static NetChannel Get(int channelId)
        {
            if (channelId >= 0 && channelId < m_channels.Count)
            {
                return m_channels[channelId];
            }
            return null;
        }

        public static void SetActiveChannel(int channelId)
        {
            currentActiveChannel = Get(channelId);
        }

        public static IEnumerator<NetChannel> All()
        {
            return m_channels.GetEnumerator();
        }

        public static void Send(int msgId)
        {
 #if PB_REFLECTION
            var descriptor = NetMsg.GetDescriptor(msgId);
            if (descriptor != null)
            {
                var protobufmsg = (Google.Protobuf.IMessage)Activator.CreateInstance(descriptor.ClrType);

                ProcessMsg(msgId, protobufmsg);

                NetMsg msg = NetMsg.Create(msgId);
                msg.ProtobufMessage = protobufmsg;

#if DEBUG_NET
                Log.Net.Print(true, "SendMsg, id={0},content={0}", msgId, msg.ToString());
#endif

                SendData(msg.EncodeToBytes());
            }
            else
            {
                Log.Net.PrintWarning("NetChannel.SendMsg, No Msg for: {0}", msgId);
            }
#endif
        }

        public static void Send(string msgName, bool ignoreCase)
        {
            Int32 msgid = NetMsg.FindMsg(msgName, ignoreCase);
            //if(ids.Length > 1)
            //{
            //    Log.Net.PrintWarning("SendMsg(string msgName), more than one msg found for name: " + msgName);
            //}
            // for(int i = 0; i < ids.Length; i++)
            {
                Send(msgid);
            }
        }

        public static void Send<T>() where T : IProtoMessage
        {
            Type t = typeof(T);
            Send(NetMsg.HashMsgID(t.FullName));
        }

        public static void Send(string msgName)
        {
            Send(msgName, true);
        }

        public static void SendData(byte[] databuffer)
        {
            if (currentActiveChannel == null)
            {
                Log.Common.PrintError("SendData Failed for Channel is Null");
                return;
            }
            if (!currentActiveChannel.m_netConnection.IsConnected())
            {
                Log.Common.PrintError("SendData Failed for Channel {0} is Closed", currentActiveChannel.ChannelId);
                return;
            }
            int bytesLen = currentActiveChannel.m_netConnection.Send(databuffer);
            currentActiveChannel.BytesWrite += bytesLen;
        }
    }

    /// <summary>
    /// net manage
    /// TODO: 重联机制
    /// </summary>
    public partial class NetChannel
    {
        INetConnection m_netConnection;
        string m_address;
        int m_port;

        /// <summary>
        /// 消息的处理频率
        /// </summary>
        public int FrameRate { get; set; }

        public long BytesWrite { get; private set; }
        public long BytesRead { get; private set; }

        public float TimeElapsed { get; private set; }

        public int ChannelId { get; private set; }
        const int MAX_BUFFER_LEN = 10 * 1024;
        byte[] m_tempBuffer;
        byte[] m_4bytesBuffer;

        public NetChannel(string iporhost, int port)
        {
            m_tempBuffer = new byte[MAX_BUFFER_LEN];
            m_4bytesBuffer = new byte[4];
            m_netConnection = new TcpConnection();
            if (string.IsNullOrEmpty(iporhost))
            {
                m_address = "127.0.0.1";
            }
            else
            {
                m_address = iporhost;
            }
            FrameRate = 1;
            Log.Common.Print("New NetChannel On {0}:{1}", iporhost, port);
            this.m_port = port;
            m_netConnection.Connect(m_address, this.m_port);

            ChannelId = channelId++;
            m_channels.Add(this);
            SetActiveChannel(ChannelId);
            Observable.EveryUpdate().Subscribe(frameUpdate);
        }

        /// <summary>
        /// HACK: NetMsg.Default，在多个channel时消息是否有问题？？
        /// </summary>
        void analyseMsg()
        {
            CirculeBuffer buffer = m_netConnection.GetDataBuffer();
            int msgLen = buffer.Length;
            if (msgLen < 4)
            {
                return;
            }
            buffer.Peek(m_4bytesBuffer, 0, 4);
            msgLen = BitConverter.ToInt32(m_4bytesBuffer, 0);
            msgLen = System.Net.IPAddress.NetworkToHostOrder(msgLen);
            if (msgLen < buffer.Length)
            {
                return;
            }

            Array.Clear(m_tempBuffer, 0, m_tempBuffer.Length);
            buffer.Read(m_tempBuffer, 0, msgLen);

            NetMsg msg = NetMsg.Default;
            msg.DecodeFromBytes(m_tempBuffer, 0, msgLen);

            ProcessMsg(msg.MsgId, msg.ProtobufMessage);
        }

        void frameUpdate(long f)
        {
            TimeElapsed += Time.unscaledDeltaTime;
            if (f % FrameRate == 0)
            {
                if (m_netConnection.Update())
                {
                    analyseMsg();
                }
            }
        }

        public void Close()
        {
            m_netConnection.Disconnect();
        }

        public string Address
        {
            get
            {
                return m_address;
            }
        }

        public int Port
        {
            get
            {
                return m_port;
            }
        }

        public INetConnection Connection
        {
            get
            {
                return m_netConnection;
            }
        }
    }
}