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
        msgheadTest();
        int ab = 1;

    }

    void Start()
    {
        basicTest();
    }

    void msgheadTest()
    {
        int headLen = MsgHead.GetLength();
        Debug.Log("MsgHead.GetLength " + headLen);
        MsgHead.Default.msgid = 123;
        MsgHead.Default.serviceid = 321;
        var datas = MsgHead.Default.Encode(100);

        Assert.AreEqual(datas.Length, headLen);

        MsgHead head = new MsgHead();
        head.Decode(datas);

        Debug.Log("head.msgid=" + head.msgid);
        Debug.Log("head.serviceid=" + head.serviceid);
        Debug.Log("head.length=" + head.length);
    }

}
