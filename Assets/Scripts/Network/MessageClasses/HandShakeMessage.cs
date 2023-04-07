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

        outData.Item1 = BitConverter.ToInt64(message, 8);
        outData.Item2 = BitConverter.ToInt32(message, 16);
        outData.Item3.r = BitConverter.ToSingle(message, 20);
        outData.Item3.g = BitConverter.ToSingle(message, 24);
        outData.Item3.b = BitConverter.ToSingle(message, 28);
        outData.Item3.a = BitConverter.ToSingle(message, 32);

        return outData;
    }

    public MESSAGE_TYPE GetMessageType()
    {
        return MESSAGE_TYPE.HAND_SHAKE;
    }

    public byte[] Serialize(float admissionTime)
    {
        List<byte> outData = new List<byte>();

        outData.AddRange(BitConverter.GetBytes((int)GetMessageType()));
        outData.AddRange(BitConverter.GetBytes(admissionTime));

        outData.AddRange(BitConverter.GetBytes(data.Item1));
        outData.AddRange(BitConverter.GetBytes(data.Item2));
        outData.AddRange(BitConverter.GetBytes(data.Item3.r));
        outData.AddRange(BitConverter.GetBytes(data.Item3.g));
        outData.AddRange(BitConverter.GetBytes(data.Item3.b));
        outData.AddRange(BitConverter.GetBytes(data.Item3.a));

        return outData.ToArray();
    }
    #endregion
}
