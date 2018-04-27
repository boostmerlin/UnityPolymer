#define DEBUG_NET

using System;
using System.Net;
using System.IO;
using Google.Protobuf;
using System.Collections.Generic;

namespace ML.Net
{
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
            if(bytes == null)
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

            byte[] data = MsgCoder.Encode(msgLength + GetLength());
            mMemoryStream.Write(data, 0, data.Length);

            data = MsgCoder.Encode(MsgId);
            mMemoryStream.Write(data, 0, data.Length);

            data = MsgCoder.Encode(ServiceId);
            mMemoryStream.Write(data, 0, data.Length);
        }

        public void DecodeHead(byte[] bytesdata)
        {
            int n = GetLength();
            if (bytesdata.Length < n)
            {
                Log.ML.PrintError("Wrong MsgHead Length");
            }
            mMemoryStream.SetLength(0);
            mMemoryStream.Write(bytesdata, 0, n);
            mMemoryStream.Seek(0, SeekOrigin.Begin);
            Length = MsgCoder.DecodeInt(mMemoryStream) - n;
            MsgId = MsgCoder.DecodeShort(mMemoryStream);
            ServiceId = MsgCoder.DecodeShort(mMemoryStream);
        //    HeadDecoded = true;
        }

        public static int GetLength()
        {
            return sizeof(int) + sizeof(short) * 2;
        }

        public override string ToString()
        {
            return string.Format("MsgHead: msgid={0}, msgLen={1}", MsgId, Length);
        }
        //public bool HeadDecoded
        //{
        //    get; protected set;
        //}
        public short MsgId { get; set; }
        public short ServiceId { get; set; }

        //消息体的长度, 不包括头长度，头部总是固定长度
        public int Length { get; private set; }
        //other ??

    }

    //
    public class NetMsg : MsgHead
    {
        static Dictionary<int, MessageParser> _parserMap = new Dictionary<int, MessageParser>();

        public static void RegParser(int msgid, MessageParser messageParser)
        {
            if(_parserMap.ContainsKey(msgid))
            {
                Log.ML.PrintWarning("[RegParser] duplicated RegParser on msgid={0}, parser={1}", msgid, messageParser.ToString());
            }
            _parserMap[msgid] = messageParser;
        }

        private NetMsg()
        {
            //can't new NetMsg()
        }

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

        public IMessage ProtobufMessage { get; set; }

        public MessageParser Parser
        {
            get;set;
        }

        public static NetMsg Create(short msgid)
        {
            NetMsg msg = new NetMsg();
            msg.MsgId = msgid;

            return msg;
        }

        public byte[] EncodeToBytes()
        {
            Log.ML.Print(true, "Construct A NetMsg Of: {0}", base.ToString());
            if (ProtobufMessage != null)
            {
#if DEBUG_NET
                Log.ML.Print(true, "Encode Protobuf Message: " + ProtobufMessage.ToString());
#endif
                var bytes = ProtobufMessage.ToByteArray();
                int len = bytes.Length;

                EncodeHead(len);
                mMemoryStream.Write(bytes, 0, len);
            }
            else
            {
                EncodeHead(0);
            }
            return mMemoryStream.ToArray();
        }

        //bytesdata should contain msghead.
        public bool DecodeBody(byte[] bytesdata)
        {
            int msgheadLen = GetLength();
            if (Length + msgheadLen > bytesdata.Length)
            {
                Log.ML.PrintError("[DecodeFromBytes] Incomplete Message data.");
                return false;
            }

            if (Parser == null)
            {
                Log.ML.PrintWarning("Set MessageParser for this message first.");
                return false;
            }
            else
            {
                ProtobufMessage = Parser.ParseFrom(bytesdata, msgheadLen, Length);
                return true;
            }
        }


        public T GetMessage<T>() where T : class, IMessage<T>
        {
            return ProtobufMessage as T;
        }

        void tryInitMessageParser()
        {
            do
            {
                int id = (int)MsgId;
                MessageParser parser;
                if (_parserMap.TryGetValue(id, out parser))
                {
                    Parser = parser;
                    break;
                }


                Log.ML.Print("Can't find any Parser for msg: {0}", id);
            } while (false);
        }

        //bytesdata should contain msghead.
        public T DecodeFromBytes<T>(byte[] bytesdata) where T : class, IMessage<T>
        {
            DecodeHead(bytesdata);

            tryInitMessageParser();

            if (DecodeBody(bytesdata) && ProtobufMessage != null)
            {
                return ProtobufMessage as T;
            }
            else
            {
                return default(T);
            }
        }
    }
}