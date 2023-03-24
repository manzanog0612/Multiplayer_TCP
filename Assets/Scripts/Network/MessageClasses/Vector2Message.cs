using System;
using System.Collections.Generic;

using UnityEngine;

public class Vector2Message : IMessage<Vector2>
{
    private Vector2 data;

    public Vector2Message(Vector2 data)
    {
        this.data = data;
    }

    public Vector2Message()
    {

    }

    public Vector2 Deserialize(byte[] message)
    {
        Vector2 outData;

        outData.x = BitConverter.ToSingle(message, 4);
        outData.y = BitConverter.ToSingle(message, 8);

        return outData;
    }

    public MESSAGE_TYPE GetMessageType()
    {
        return MESSAGE_TYPE.VECTOR2;
    }

    public byte[] Serialize()
    {
        List<byte> outData = new List<byte>();

        outData.AddRange(BitConverter.GetBytes((int)GetMessageType()));

        outData.AddRange(BitConverter.GetBytes(data.x));
        outData.AddRange(BitConverter.GetBytes(data.y));

        return outData.ToArray();
    }
}
