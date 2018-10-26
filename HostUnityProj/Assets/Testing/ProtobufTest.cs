using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Protobuf;
using Google.Protobuf.Examples.AddressBook;
using System.IO;
using System;
using Ginkgo.Net;
using UnityEngine.Assertions;
using System.Reflection;
using Hello;
using Ginkgo;

public class MsgService : BaseMsgService
{
    [NetSender(Package = "Hello")]
    public void HelloRequest(IMessage req)
    {
        Debug.Log("HelloRequest Send");
        var msg = req as HelloRequest;
        msg.Greeting = "hello server";
    }

    public void hello_HelloRequest(IMessage req)
    {

    }

    [NetHandler]
    public void HelloResponse(Hello.HelloResponse res)
    {
        Debug.Log("HelloResponse : " + res.ToString());
    }
}

public class ProtobufTest : MonoBehaviour
{
    NetChannel netChannel;
    void basicTest()
    {
        var assembly = Assembly.GetExecutingAssembly();
        Debug.Log(assembly.FullName);
        byte[] bytes;
        // Create a new person
        Person person = new Person
        {
            Id = 1,
            Name = "Foo",
            Email = "foo@bar",
            Phones = { new Person.Types.PhoneNumber { Number = "555-1212" } }
        };
        using (MemoryStream stream = new MemoryStream())
        {
            // Save the person to a stream
            person.WriteTo(stream);
            bytes = stream.ToArray();
        }
        Person copy = Person.Parser.ParseFrom(bytes);
        AddressBook book = new AddressBook
        {
            People = { copy }
        };
        bytes = book.ToByteArray();
        // And read the address book back again
        AddressBook restored = AddressBook.Parser.ParseFrom(bytes);
        var a = AddressBook.Descriptor;
        // The message performs a deep-comparison on equality:
        if (restored.People.Count != 1 || !person.Equals(restored.People[0]))
        {
            throw new Exception("There is a bad person in here!");
        }
        Debug.Log(restored.ToString());

        var r = Hello.HelloRpcReflection.Descriptor;
        
        Debug.Log(r.ToString());
    }

    void Start()
    {
        Debug.Log("Protobuf Test Begin....");
        basicTest();
        netChannel = new NetChannel("127.0.0.1", 9999);
        var tcpConnection = netChannel.Connection;
        tcpConnection.OnConnect += new SocketStateHandler((ss, ok) =>
        {
            Debug.LogFormat("connect state: {0}, ok?{1}", ss.ToString(), ok);
        });
        tcpConnection.OnStateReport += new SocketMessageHandler((msg) =>
        {
            Debug.Log(msg);
        });
        StartCoroutine(delayExecute());
    }

    IEnumerator delayExecute()
    {
        yield return new WaitForSeconds(0.5f);

        msgTest();
    }

    private void OnDestroy()
    {
        netChannel.Close();
    }

    private void OnGUI()
    {
        if(GUILayout.Button("HelloRequest"))
        {
            //  NetChannel.SendMsg("HelloRequest");
            NetChannel.Send<HelloRequest>();
        }
    }

    void msgTest()
    {
        int headLen = MsgHead.GetLength();
        Debug.Log("MsgHead.GetLength " + headLen);
        MsgHead.DefaultHead.MsgId = 123;
        MsgHead.DefaultHead.EncodeHead(100);
        var datas = MsgHead.DefaultHead.GetBytes();

        Assert.AreEqual(datas.Length, headLen);

        MsgHead head = MsgHead.DefaultHead;
        head.DecodeHead(datas);

        Debug.Log("head.msgid=" + head.MsgId);
        Debug.Log("head.length=" + head.Length);


        Person person = new Person
        {
            Id = 1,
            Name = "Foo",
            Email = "foo@bar.com",
            Phones = { new Person.Types.PhoneNumber { Number = "555-1212" } }
        };
        NetMsg msg = NetMsg.Create();
        msg.ProtobufMessage = person;

        Debug.Log(Person.Descriptor.ClrType.FullName);
        Debug.Log(Person.Descriptor.Name);
        Debug.Log(Person.Descriptor.File.Package);
        foreach(var f in Person.Descriptor.Fields.InFieldNumberOrder())
        {
            Debug.Log(f.FullName);
        }
        Assert.AreEqual(Person.Descriptor.Parser, Person.Parser);

        datas = msg.EncodeToBytes();
        Debug.Log("datas = " + datas.Length);

        NetMsg.Default.DecodeFromBytes<Person>(datas);

        Debug.Log(NetMsg.Default.ToString());
    }
}
