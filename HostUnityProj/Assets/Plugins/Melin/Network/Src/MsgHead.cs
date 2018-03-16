using System;
using System.Net;
using System.IO;

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
        static MemoryStream stream = new MemoryStream();
        public static MsgHead Default
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

        public byte[] Encode(int msgLength)
        {
            stream.SetLength(0);

            byte[] data = MsgCoder.Encode(msgLength + GetLength());
            stream.Write(data, 0, data.Length);

            data = MsgCoder.Encode(msgid);
            stream.Write(data, 0, data.Length);

            data = MsgCoder.Encode(serviceid);
            stream.Write(data, 0, data.Length);

            return stream.ToArray();
        }

        public void Decode(byte[] head)
        {
            int n = GetLength();
            if (head.Length < n)
            {
                Log.ML.PrintError("Wrong MsgHead Length");
            }
            stream.SetLength(0);
            stream.Write(head, 0, n);
            stream.Seek(0, SeekOrigin.Begin);
            length = MsgCoder.DecodeInt(stream) - n;
            msgid = MsgCoder.DecodeShort(stream);
            serviceid = MsgCoder.DecodeShort(stream);
        }

        public static int GetLength()
        {
            return sizeof(int) + sizeof(short) * 2;
        }

        public short msgid;
        public short serviceid;
        public int length;
        //other ?? 
    }
}