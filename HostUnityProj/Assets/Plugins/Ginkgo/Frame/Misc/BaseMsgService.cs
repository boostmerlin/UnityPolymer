using System.Collections.Generic;
using System;
using System.Reflection;
using Google.Protobuf;

namespace Ginkgo
{
    public class BaseNetAttribute : Attribute
    {
        public Int32 MsgId { get; set; }
        public string Package { get; set; }
    }

    /// <summary>
    /// 指定发送消息处理函数，需定义为public有效
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class NetSenderAttribute : BaseNetAttribute
    {
    }

    /// <summary>
    /// 指定接收消息处理函数，需定义为public有效
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class NetHandlerAttribute : BaseNetAttribute
    {
    }

    public delegate void MsgProcessor<T>(T message) where T : IMessage<T>;
    public delegate void MsgProcessor(IMessage message);

    public abstract class BaseMsgService : IEquatable<BaseMsgService>
    {
        Dictionary<Int32, MsgProcessor> m_methodMap;

        public string FullName { get; set; }
        public IDictionary<Int32, MsgProcessor> GetMsgProcessors()
        {
            return m_methodMap;
        }

        protected void RegProcessor(Int32 msgId, MsgProcessor msgProcessor)
        {
            if (m_methodMap.ContainsKey(msgId))
            {
                Log.Common.PrintWarning("Multiple Set MsgProcessor On Msg:{0} in one MsgService!", msgId);
            }

            m_methodMap[msgId] = msgProcessor;
        }

        public void CallProcessor(Int32 msgId, IMessage message)
        {
            if(m_methodMap.ContainsKey(msgId))
            {
                m_methodMap[msgId](message);
            }
            else
            {
                Log.Net.PrintWarning("BaseMsgService.CallProcessor, something not right, no processor found for msg: " + msgId);
            }
        }

        public bool Equals(BaseMsgService other)
        {
            return FullName.Equals(other.FullName);
        }

        protected BaseMsgService()
        {
            const string DT_ID = "BaseMsgService Constructor";
            DebugTimer.BEGIN(DT_ID);
            m_methodMap = new Dictionary<Int32, MsgProcessor>();
            bool usePrebind = GinkgoConfig.Selfie.PreBindCode;
            if(usePrebind)
            {
                return;
            }
            Type thisType = GetType();
            FullName = thisType.FullName;
            MethodInfo[] methods = thisType.GetMethods(BindingFlags.Instance 
                | BindingFlags.Public 
                | BindingFlags.DeclaredOnly);
            Type iMessageType = typeof(IMessage);
            foreach(var mi in methods)
            {
                MethodInfo methodInfo = mi;
                var attributes = methodInfo.GetCustomAttributes(true);
                foreach(var attribute in attributes)
                {
                    BaseNetAttribute nsa = attribute as BaseNetAttribute;
                    if(nsa != null)
                    {
                        Int32 msgId = nsa.MsgId;
                        ParameterInfo[] pis = methodInfo.GetParameters();
                        if(pis.Length != 1)
                        {
                            Log.Common.PrintWarning("NetSenderAttribute or NetHandlerAttribute can't only apply to method have one parameter.");
                            continue;
                        }

                        Type parameterType = pis[0].ParameterType;
                        if (!iMessageType.IsAssignableFrom(parameterType))
                        {
                            Log.Common.PrintWarning("MsgProcessor's parameter is NOT Google.Protobuf.IMessage.");
                            continue;
                        }

                        string msgName = parameterType.FullName;
                        if (msgId == Net.NetMsg.INVALID_MSGID)
                        {
                            //generate msg id from method parameter
                            if (iMessageType != parameterType)
                            {
                                msgId = Net.NetMsg.HashMsgID(msgName);
                            }
                            else //generate msg id from method name
                            {
                                msgName = mi.Name.Replace('_', '.');
                                if(!string.IsNullOrEmpty(nsa.Package))
                                {
                                    msgName = string.Format("{0}.{1}", nsa.Package, msgName);
                                }
                                msgId = Net.NetMsg.HashMsgID(msgName);
                            }
                        }
                        if(msgId != Net.NetMsg.INVALID_MSGID)
                        {
                            if(Net.NetMsg.GetDescriptor(msgId) == null)
                            {
                                Log.Common.PrintWarning("RegProcessor failed,no msg: [{0}]. method={1}, parameter={2}", msgId, mi.Name, msgName);
                                continue;
                            }

                            RegProcessor(msgId, (message) =>
                            {
                                methodInfo.Invoke(this, new object[] { message });
                            });
                        }
                    }
                }
            }
            DebugTimer.END(DT_ID);
        }

        //other 
    }
}
