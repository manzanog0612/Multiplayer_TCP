using System;
using System.Collections.Generic;

using UnityEngine;

public class Vector2Message : IMessage<Vector2>
{
    static public int lastMessageId = 0;

    #region PRIVATE_FIELDS
    private Vector2? data = null;
    #endregion

    #region CONSTRUCTORS
    public Vector2Message() { }

    public Vector2Message(Vector2 data)
    {
        this.data = data;
    }
    #endregion

    #region PUBLIC_METHODS
    public static Vector2 Deserialize(byte[] message)
    {
        Vector2 outData;

        outData.x = BitConverter.ToSingle(message, GetHeaderSize());
        outData.y = BitConverter.ToSingle(message, GetHeaderSize() + sizeof(float));

        return outData;
    }

    public MessageHeader GetMessageHeader(float admissionTime)
    {
        return new MessageHeader((int)GetMessageType(), admissionTime, lastMessageId);
    }

    public static MESSAGE_TYPE GetMessageType()
    {
        return MESSAGE_TYPE.VECTOR2;
    }

    public byte[] Serialize(float admissionTime)
    {
        List<byte> outData = new List<byte>();

        lastMessageId++;
        MessageHeader messageHeader = GetMessageHeader(admissionTime);

        outData.AddRange(messageHeader.Bytes);
        outData.AddRange(BitConverter.GetBytes(((Vector2)data).x));
        outData.AddRange(BitConverter.GetBytes(((Vector2)data).y));

        return outData.ToArray();
    }

    public static int GetHeaderSize()
    {
        return sizeof(bool) + sizeof(int) * MessageHeader.amountIntsInSendTime + sizeof(int) + sizeof(float) + sizeof(int);
    }

    public static int GetMessageSize()
    {
        throw new NotImplementedException();
    }
    #endregion
}
