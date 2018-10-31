#define DEBUG_NET

//#define PB_REFLECTION

using System;
using System.Net;
using System.IO;
using System.Collections.Generic;
using IProtoMessage = Google.Protobuf.IMessage;
using MessageParser = Google.Protobuf.MessageParser;
using Google.Protobuf;

namespace Ginkgo.Net
{
    using ID = System.Int32;

    public static class MsgCoder
    {
        static byte[] buffer = new byte[8];
        public static byte[] Encode(int n)
        {
            int nn = IPAddress.HostToNetworkOrder(n);
            byte[] data = BitConverter.GetBytes(nn);
            return data;
        }
        public static byte[] Encode(short n)
        {
            short nn = IPAddress.HostToNetworkOrder(n);
            byte[] data = BitConverter.GetBytes(nn);
            return data;
        }

        public static byte[] Encode(long n)
        {
            long nn = IPAddress.HostToNetworkOrder(n);
            byte[] data = BitConverter.GetBytes(nn);
            return data;
        }

        public static byte[] Encode(byte[] bytes)
        {
            if (bytes == null)
            {
                return new byte[0];
            }
            int len = bytes.Length;
            MemoryStream ms = new MemoryStream();
            ms.Write(Encode(len), 0, sizeof(int));
            ms.Write(bytes, 0, len);
            return ms.GetBuffer();
        }

        public static byte[] Encode(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return new byte[0];
            }
            byte[] strBytes = System.Text.UTF8Encoding.Default.GetBytes(str);
            return Encode(strBytes);
        }

        public static short DecodeShort(MemoryStream stream)
        {
            stream.Read(buffer, 0, sizeof(short));
            var n = BitConverter.ToInt16(buffer, 0);
            return IPAddress.NetworkToHostOrder(n);
        }

        public static int DecodeInt(MemoryStream stream)
        {
            stream.Read(buffer, 0, sizeof(int));
            var n = BitConverter.ToInt32(buffer, 0);
            return IPAddress.NetworkToHostOrder(n);
        }

        public static long DecodeLong(MemoryStream stream)
        {
            stream.Read(buffer, 0, sizeof(long));
            var n = BitConverter.ToInt64(buffer, 0);
            return IPAddress.NetworkToHostOrder(n);
        }
    }

    /// <summary>
    /// 头8个字节总是消息总长度和msgid
    /// 其它:
    /// 
    /// </summary>
    public class MsgHead
    {
        static MsgHead _default;
        protected MemoryStream mMemoryStream = new MemoryStream();
        public static MsgHead DefaultHead
        {
            get
            {
                if (_default == null)
                {
                    _default = new MsgHead();
                }
                return _default;
            }
        }

        protected MsgHead()
        {

        }

        public byte[] GetBytes()
        {
            return mMemoryStream.ToArray();
        }

        public void EncodeHead(int msgLength)
        {
            mMemoryStream.SetLength(0);

            Length = msgLength + GetLength();
            byte[] data = MsgCoder.Encode(Length);
            mMemoryStream.Write(data, 0, data.Length);

            data = MsgCoder.Encode((ID)MsgId);
            mMemoryStream.Write(data, 0, data.Length);
        }

        public bool DecodeHead(byte[] bytesdata)
        {
            return DecodeHead(bytesdata, 0, bytesdata.Length);
        }

        public bool DecodeHead(byte[] bytesdata, int offset, int length)
        {
            int n = GetLength();
            if (length < n)
            {
                Log.Common.PrintError("Wrong bytesdata when decoding, too small data.");
                return false;
            }
            mMemoryStream.SetLength(0);
            mMemoryStream.Write(bytesdata, offset, n);
            mMemoryStream.Seek(0, SeekOrigin.Begin);
            Length = MsgCoder.DecodeInt(mMemoryStream);
            if (Length > MAX_LENGTH || Length < 0)
            {
                Log.Common.PrintError("Wrong MsgHead Length, value={0}, max length: {1}" , Length, MAX_LENGTH);
                return false;
            }
            MsgId = (ID)MsgCoder.DecodeInt(mMemoryStream);
            //    HeadDecoded = true;
            return true;
        }

        public override string ToString()
        {
            return string.Format("MsgHead: msgid={0}, MsgLen={1}, headLen={2}\n"
                , MsgId
                , Length
                , GetLength());
        }
        //public bool HeadDecoded
        //{
        //    get; protected set;
        //}
        public static int GetLength()
        {
            return sizeof(int) + sizeof(ID);
        }
        //消息体总长度
        public int Length { get; private set; }
        //消息识别号
        public ID MsgId { get; set; }
        //消息体的最大长度, 不包括头长度，头部总是固定长度
        const int MAX_LENGTH = 1024 * 1024 * 10;

        //other ??
    }

    //
    public class NetMsg : MsgHead
    {
#if PB_REFLECTION
        static Dictionary<ID, MessageDescriptor> _descriptorMap = new Dictionary<ID, MessageDescriptor>();
        public static MessageDescriptor GetDescriptor(ID msgId)
        {
            MessageDescriptor find;
            _descriptorMap.TryGetValue(msgId, out find);
            return find;
        }
#endif

        static bool StringContains(string src, string val, bool ignoreCase)
        {
            bool find = false;
            if (ignoreCase)
            {
                find = src.Contains(val, StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                find = src.Contains(val);
            }
            return find;
        }

        public static ID HashMsgID(string fullName)
        {
            if(string.IsNullOrEmpty(fullName))
            {
                return INVALID_MSGID;
            }

            return GeneralUtils.HashFNV(fullName);
        }

        public static ID FindMsg(string partialMsgName, bool ignoreCase)
        {
#if PB_REFLECTION
#if DEBUG_NET
            int matchCount = 0;
            _descriptorMap.Values.ForEach((descriptor)=> {
                string msgname = descriptor.FullName;

                if (StringContains(msgname, partialMsgName, ignoreCase))
                {
                    matchCount++;
                }
            });
            if(matchCount > 1)
            {
                Log.Net.PrintWarning("[FindMsg], more than one msg found for name: " + partialMsgName);
            }
#endif

            foreach (var kv in _descriptorMap)
            {
                if (StringContains(kv.Value.FullName, partialMsgName, ignoreCase))
                {
                    return kv.Key;
                }
            }
#endif
            return INVALID_MSGID;
        }

        //public static ID[] FindMsg(string partialMsgName)
        //{
        //    List<ID> ids = new List<ID>();
        //    foreach(var kv in _descriptorMap)
        //    {
        //        if(kv.Value.FullName.Contains(partialMsgName))
        //        {
        //            ids.Add(kv.Key);
        //        }
        //    }

//    return ids.ToArray();
//}

        /// <summary>
        /// 对于相同的msgid，不会覆盖掉旧的
        /// </summary>
        /// <param name="msgid"></param>
        /// <param name="MessageDescriptor"></param>
#if PB_REFLECTION
        public static void RegDescriptor(ID msgid, MessageDescriptor messageDescriptor)
        {
            MessageDescriptor find;
            if (_descriptorMap.TryGetValue(msgid, out find))
            {
                if (find != messageDescriptor)
                {
                    Log.Common.PrintWarning("[RegDescriptor] duplicated MessageDescriptor on msgid={0}, descriptor={1}", msgid, messageDescriptor.Name);
                }
            }
            else
            {
#if DEBUG_NET
                Log.Common.Print(true, "[RegDescriptor] reg on msgid={0}, Descriptor ClrType={1}", msgid, messageDescriptor.ClrType.FullName);
#endif
                _descriptorMap[msgid] = messageDescriptor;
            }
        }
#endif

        private NetMsg()
        {
            //can't new NetMsg()
        }

        public const ID INVALID_MSGID = 0;

        static NetMsg _default;
        public static NetMsg Default
        {
            get
            {
                if (_default == null)
                {
                    _default = new NetMsg();
                }
                return _default;
            }
        }

        public MsgHead MsgHead
        {
            get
            {
                return this;
            }
        }

        public IProtoMessage ProtobufMessage { get; set; }

        public MessageParser Parser
        {
            get; set;
        }

        public static NetMsg Create(ID msgid)
        {
            NetMsg msg = new NetMsg
            {
                MsgId = (ID)msgid
            };

            return msg;
        }

        public static NetMsg Create()
        {
            NetMsg msg = new NetMsg
            {
                MsgId = INVALID_MSGID
            };

            return msg;
        }

        public static NetMsg Create(IProtoMessage protoMessage)
        {
            NetMsg msg = new NetMsg
            {
                MsgId = INVALID_MSGID,
                ProtobufMessage = protoMessage
            };

            return msg;
        }

        public byte[] EncodeToBytes()
        {
            if (ProtobufMessage != null)
            {
                if (MsgId == INVALID_MSGID)
                {
               #if PB_REFLECTION
                    var descriptor = ProtobufMessage.Descriptor;
                    MsgId = NetMsg.HashMsgID(descriptor.ClrType.FullName);
              #endif
                }

                var bytes = ProtobufMessage.ToByteArray();
                int len = bytes.Length;

                EncodeHead(len);
                mMemoryStream.Write(bytes, 0, len);
            }
            else
            {
                EncodeHead(0);
            }
#if DEBUG_NET
            Log.Common.Print(true, "Encode Protobuf Message=" + ToString());
#endif
            return mMemoryStream.ToArray();
        }

        //bytesdata should contain msghead.
        public bool DecodeBody(byte[] bytesdata, int offset, int length)
        {
            if (Length > length)
            {
                Log.Common.PrintError("[DecodeFromBytes] Incomplete Message data.");
                return false;
            }

            if (Parser == null)
            {
                Log.Common.PrintWarning("Set MessageParser for this message first.");
                return false;
            }
            else
            {
                int msgheadLen = GetLength();
                //ProtobufMessage = Parser.ParseFrom(bytesdata, msgheadLen + offset, Length - msgheadLen);
                return true;
            }
        }

        public T GetMessage<T>() where T : class, IProtoMessage
        {
            return ProtobufMessage as T;
        }

        void initMessageParser()
        {
            Parser = null;
            if (Parser == null)
            {
                int id = (ID)MsgId;
#if PB_REFLECTION
                MessageDescriptor descriptor;
                if (_descriptorMap.TryGetValue(id, out descriptor))
                {
                    Parser = descriptor.Parser;
                }
                else
                {
                    Log.Common.Print("Can't find any Parser for msg: {0}", id);
                }
#endif
            }
        }

        /// <summary>
        /// bytesdata should contain msghead.
        /// </summary>
        /// <param name="bytesdata"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public T DecodeFromBytes<T>(byte[] bytesdata, int offset, int length) where T : class, IProtoMessage
        {
            return DecodeFromBytes(bytesdata, offset, length) as T;
        }

        /// <summary>
        /// bytesdata should contain msghead.
        /// </summary>
        public T DecodeFromBytes<T>(byte[] bytesdata) where T : class, IProtoMessage
        {
            return DecodeFromBytes(bytesdata, 0, bytesdata.Length) as T;
        }

        /// <summary>
        /// bytesdata should contain msghead.
        /// </summary>
        /// <param name="bytesdata"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public IProtoMessage DecodeFromBytes(byte[] bytesdata)
        {
            return DecodeFromBytes(bytesdata, 0, bytesdata.Length);
        }

        /// <summary>
        /// bytesdata should contain msghead.
        /// </summary>
        public IProtoMessage DecodeFromBytes(byte[] bytesdata, int offset, int length)
        {
            ProtobufMessage = null;
            if (!DecodeHead(bytesdata, offset, length))
            {
                return ProtobufMessage;
            }

            initMessageParser();

            DecodeBody(bytesdata, offset, length);
            return ProtobufMessage;
        }

        public override string ToString()
        {
            string s = base.ToString();
            if(ProtobufMessage != null)
            {
                s += ProtobufMessage.ToString();
            }
            return s;
        }
    }
}