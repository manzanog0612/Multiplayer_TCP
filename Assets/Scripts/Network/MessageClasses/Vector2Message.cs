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
    public Vector2 Deserialize(byte[] message)
    {
        Vector2 outData;

        outData.x = BitConverter.ToSingle(message, 12);
        outData.y = BitConverter.ToSingle(message, 16);

        return outData;
    }

    public MESSAGE_TYPE GetMessageType()
    {
        return MESSAGE_TYPE.VECTOR2;
    }

    public byte[] Serialize(float admissionTime)
    {
        List<byte> outData = new List<byte>();

        outData.AddRange(BitConverter.GetBytes((int)GetMessageType()));
        outData.AddRange(BitConverter.GetBytes(admissionTime));
        outData.AddRange(BitConverter.GetBytes(lastMessageId++));

        outData.AddRange(BitConverter.GetBytes(((Vector2)data).x));
        outData.AddRange(BitConverter.GetBytes(((Vector2)data).y));

        return outData.ToArray();
    }
    #endregion
}
