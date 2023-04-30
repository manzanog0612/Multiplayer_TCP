using System;
using System.Collections.Generic;
using UnityEngine;

public class HandShakeMessage : IMessage<(long, int, Color)>
{
    #region PRIVATE_FIELDS
    private (long, int, Color) data;
    #endregion

    #region CONSTRUCTORS
    public HandShakeMessage() { }

    public HandShakeMessage((long, int, Color) data)
    {
        this.data = data;
    }
    #endregion

    #region PUBLIC_METHODS
    public (long, int, Color) Deserialize(byte[] message)
    {
        (long, int, Color) outData;

        outData.Item1 = BitConverter.ToInt64(message, GetHeaderSize());
        outData.Item2 = BitConverter.ToInt32(message, GetHeaderSize() + sizeof(long));
        outData.Item3.r = BitConverter.ToSingle(message, GetHeaderSize() + sizeof(long) + sizeof(int));
        outData.Item3.g = BitConverter.ToSingle(message, GetHeaderSize() + sizeof(long) + sizeof(int) + sizeof(float));
        outData.Item3.b = BitConverter.ToSingle(message, GetHeaderSize() + sizeof(long) + sizeof(int) + sizeof(float) * 2);
        outData.Item3.a = BitConverter.ToSingle(message, GetHeaderSize() + sizeof(long) + sizeof(int) + sizeof(float) * 3);

        return outData;
    }

    public MessageHeader GetMessageHeader(float admissionTime)
    {
        return new MessageHeader((int)GetMessageType(), admissionTime);
    }

    public MESSAGE_TYPE GetMessageType()
    {
        return MESSAGE_TYPE.HAND_SHAKE;
    }

    public byte[] Serialize(float admissionTime)
    {
        List<byte> outData = new List<byte>();

        MessageHeader messageHeader = GetMessageHeader(admissionTime);

        outData.AddRange(messageHeader.Bytes);

        outData.AddRange(BitConverter.GetBytes(data.Item1));
        outData.AddRange(BitConverter.GetBytes(data.Item2));
        outData.AddRange(BitConverter.GetBytes(data.Item3.r));
        outData.AddRange(BitConverter.GetBytes(data.Item3.g));
        outData.AddRange(BitConverter.GetBytes(data.Item3.b));
        outData.AddRange(BitConverter.GetBytes(data.Item3.a));

        return outData.ToArray();
    }

    public int GetHeaderSize()
    {
        return sizeof(float) + sizeof(int) + sizeof(float);
    }
    #endregion
}
