using System;
using System.Collections.Generic;

using UnityEngine;

public class Vector3Message : IMessage<Vector3>
{
    static public int lastMessageId = 0;

    #region PRIVATE_FIELDS
    private Vector3? data = null;
    #endregion

    #region CONSTRUCTORS
    public Vector3Message() { }

    public Vector3Message(Vector3 data)
    {
        this.data = data;
    }
    #endregion

    #region PUBLIC_METHODS
    public static Vector3 Deserialize(byte[] message)
    {
        Vector3 outData;

        outData.x = BitConverter.ToSingle(message, GetHeaderSize());
        outData.y = BitConverter.ToSingle(message, GetHeaderSize() + sizeof(float));
        outData.z = BitConverter.ToSingle(message, GetHeaderSize() + sizeof(float) * 2);

        return outData;
    }

    public MessageHeader GetMessageHeader(float admissionTime)
    {
        return new MessageHeader((int)GetMessageType(), admissionTime, lastMessageId);
    }

    public static MESSAGE_TYPE GetMessageType()
    {
        return MESSAGE_TYPE.VECTOR3;
    }

    public byte[] Serialize(float admissionTime = -1)
    {
        List<byte> outData = new List<byte>();

        lastMessageId++;
        MessageHeader messageHeader = GetMessageHeader(admissionTime);

        outData.AddRange(messageHeader.Bytes);

        outData.AddRange(BitConverter.GetBytes(((Vector3)data).x));
        outData.AddRange(BitConverter.GetBytes(((Vector3)data).y));
        outData.AddRange(BitConverter.GetBytes(((Vector3)data).z));

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
