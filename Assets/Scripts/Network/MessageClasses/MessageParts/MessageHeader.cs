using System;
using System.Collections.Generic;
using UnityEngine;

public class MessageHeader
{
    public int messageType = 0;
    public float admissionTime = -1;
    public int lastMessageId = -1;
    public float sendTime = 0;

    public byte[] Bytes { get; private set; }

    public MessageHeader(int messageType, float admissionTime, int lastMessageId)
    {
        this.messageType = messageType;
        this.admissionTime = admissionTime;
        this.lastMessageId = lastMessageId;
        sendTime = Time.realtimeSinceStartup;

        List<byte> bytes = new List<byte>();

        bytes.AddRange(BitConverter.GetBytes(sendTime));
        bytes.AddRange(BitConverter.GetBytes(messageType));
        bytes.AddRange(BitConverter.GetBytes(admissionTime));
        bytes.AddRange(BitConverter.GetBytes(lastMessageId));

        Bytes = bytes.ToArray();
    }

    public MessageHeader(int messageType, float admissionTime)
    {
        this.messageType = messageType;
        this.admissionTime = admissionTime;
        sendTime = Time.realtimeSinceStartup;

        List<byte> bytes = new List<byte>();

        bytes.AddRange(BitConverter.GetBytes(sendTime));
        bytes.AddRange(BitConverter.GetBytes(messageType));
        bytes.AddRange(BitConverter.GetBytes(admissionTime));

        Bytes = bytes.ToArray();

        
    }

    public MessageHeader(int messageType)
    {
        this.messageType = messageType;
        sendTime = Time.realtimeSinceStartup;

        List<byte> bytes = new List<byte>();

        bytes.AddRange(BitConverter.GetBytes(sendTime));
        bytes.AddRange(BitConverter.GetBytes(messageType));

        Bytes = bytes.ToArray();

        sendTime = Time.realtimeSinceStartup;
    }
}
