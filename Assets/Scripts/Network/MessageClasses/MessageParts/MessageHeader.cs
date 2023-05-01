using System;
using System.Collections.Generic;
using System.Security.Cryptography;

public class MessageHeader
{
    public int messageType = 0;
    public float admissionTime = -1;
    public int lastMessageId = -1;
    public (int days, int hours, int minutes, int seconds, int millisecond) sendTime;

    public static int amountIntsInSendTime = 5;

    public byte[] Bytes { get; private set; }

    public MessageHeader(int messageType, float admissionTime, int lastMessageId)
    {
        this.messageType = messageType;
        this.admissionTime = admissionTime;
        this.lastMessageId = lastMessageId;
        sendTime = (DateTime.UtcNow.Day, DateTime.UtcNow.Hour, DateTime.UtcNow.Minute, DateTime.UtcNow.Second, DateTime.UtcNow.Millisecond);

        List<byte> bytes = new List<byte>();

        bytes.AddRange(GetSendTimeBytes());
        bytes.AddRange(BitConverter.GetBytes(messageType));
        bytes.AddRange(BitConverter.GetBytes(admissionTime));
        bytes.AddRange(BitConverter.GetBytes(lastMessageId));

        Bytes = bytes.ToArray();
    }

    public MessageHeader(int messageType, float admissionTime)
    {
        this.messageType = messageType;
        this.admissionTime = admissionTime;
        sendTime = (DateTime.UtcNow.Day, DateTime.UtcNow.Hour, DateTime.UtcNow.Minute, DateTime.UtcNow.Second, DateTime.UtcNow.Millisecond);

        List<byte> bytes = new List<byte>();

        bytes.AddRange(GetSendTimeBytes());
        bytes.AddRange(BitConverter.GetBytes(messageType));
        bytes.AddRange(BitConverter.GetBytes(admissionTime));

        Bytes = bytes.ToArray();


    }

    public MessageHeader(int messageType)
    {
        this.messageType = messageType;
        sendTime = (DateTime.UtcNow.Day, DateTime.UtcNow.Hour, DateTime.UtcNow.Minute, DateTime.UtcNow.Second, DateTime.UtcNow.Millisecond);

        List<byte> bytes = new List<byte>();

        bytes.AddRange(GetSendTimeBytes());
        bytes.AddRange(BitConverter.GetBytes(messageType));

        Bytes = bytes.ToArray();
    }

    private byte[] GetSendTimeBytes()
    {
        List<byte> bytes = new List<byte>();

        bytes.AddRange(BitConverter.GetBytes(sendTime.days));
        bytes.AddRange(BitConverter.GetBytes(sendTime.hours));
        bytes.AddRange(BitConverter.GetBytes(sendTime.minutes));
        bytes.AddRange(BitConverter.GetBytes(sendTime.seconds));
        bytes.AddRange(BitConverter.GetBytes(sendTime.millisecond));

        return bytes.ToArray();
    }
}
