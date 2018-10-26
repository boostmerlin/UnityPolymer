using System.Collections;
using UniRx;
using UnityEngine;
using Google.Protobuf;
using Ginkgo.Net;
using Google.Protobuf.Reflection;

namespace Ginkgo
{
    public class NetworkService : IService
    {
        public IEventAggregator EventAggregator
        {
            get
            {
                return MSystem.EventAggregator;
            }
        }

        public void Loaded()
        {
        }

        public IObservable<TEvent> OnEvent<TEvent>()
        {
            return null;
        }

        public void Publish<TEvent>(TEvent eventMessage)
        {
            EventAggregator.Publish(eventMessage);
        }

        public void Setup()
        {
           
        }

        void registerMsgSerivce()
        {
            Log.Common.Print("[Protobuf Service] Register BaseMsgService Type.");
            var msgctrls = ReflectionUtil.FindTypesInAssembly(new string[] { GinkgoConfig.Selfie.assemblyName }, typeof(BaseMsgService));
            foreach (var msgctrl in msgctrls)
            {
                NetChannel.RegMsgService(System.Activator.CreateInstance(msgctrl) as BaseMsgService);
            }
        }

        void registerDescriptor()
        {
            Log.Common.Print("[Protobuf Service] Register PB Descriptor.");
            //以后可能有代码生成方式注册
            var protos = ReflectionUtil.FindTypesInAssembly(new string[] { GinkgoConfig.Selfie.assemblyName }, typeof(IMessage));
            foreach (var proto in protos)
            {
                var fi = proto.GetProperty("Descriptor", System.Reflection.BindingFlags.GetProperty
                    | System.Reflection.BindingFlags.Public
                    | System.Reflection.BindingFlags.Static);
                if (fi != null)
                {
                    var descriptor = fi.GetValue(null, null) as MessageDescriptor;
                    int msgid = NetMsg.HashMsgID(proto.FullName);
                    NetMsg.RegDescriptor(msgid, descriptor);
                }
            }
        }

        public IEnumerator SetupAsync()
        {
            registerDescriptor();
            yield return 0;
            registerMsgSerivce();

            Publish(new Event.ProtoRegisterOver());
        }
    }
}
