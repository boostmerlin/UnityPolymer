using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Protobuf;
using Google.Protobuf.Examples.AddressBook;
using System.IO;
using System;

public class ProtobufTest : MonoBehaviour
{

    // Use this for initialization
    void Start()
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
    }

    // Update is called once per frame
    void Update()
    {

    }
}
