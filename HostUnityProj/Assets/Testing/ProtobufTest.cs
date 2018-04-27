using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Protobuf;
using Google.Protobuf.Examples.AddressBook;
using System.IO;
using System;
using ML.Net;
using UnityEngine.Assertions;

public class ProtobufTest : MonoBehaviour
{
    TcpConnection tcpConnection;
    void basicTest()
    {
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
        basicTest();
        msgTest();
        tcpConnection = new TcpConnection();
        tcpConnection.OnConnect += new ConnectionHandler((ss, ok) =>
        {
            Debug.LogFormat("connect state: {0}, {1}", ss.ToString(), ok);
        });
        tcpConnection.OnStateReport += new StateReportHandler((msg) =>
        {
            Debug.LogFormat(msg);
        });
    }

    private void OnDestroy()
    {
        tcpConnection.Disconnect();
    }

    void netConnect()
    {
        tcpConnection.BeginConnect("127.0.0.1", 9999);
    }

    private void OnGUI()
    {
        if(GUILayout.Button("Connect"))
        {
            netConnect();
        }
    }


    void msgTest()
    {
        int headLen = MsgHead.GetLength();
        Debug.Log("MsgHead.GetLength " + headLen);
        MsgHead.DefaultHead.MsgId = 123;
        MsgHead.DefaultHead.ServiceId = 321;
        MsgHead.DefaultHead.EncodeHead(100);
        var datas = MsgHead.DefaultHead.GetBytes();

        Assert.AreEqual(datas.Length, headLen);

        MsgHead head = MsgHead.DefaultHead;
        head.DecodeHead(datas);

        Debug.Log("head.msgid=" + head.MsgId);
        Debug.Log("head.serviceid=" + head.ServiceId);
        Debug.Log("head.length=" + head.Length);


        Person person = new Person
        {
            Id = 1,
            Name = "Foo",
            Email = "foo@bar",
            Phones = { new Person.Types.PhoneNumber { Number = "555-1212" } }
        };
        NetMsg msg = NetMsg.Create(111);
        msg.ProtobufMessage = person;
    }
}
