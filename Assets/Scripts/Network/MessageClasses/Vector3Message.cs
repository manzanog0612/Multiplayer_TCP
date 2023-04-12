using System;
using System.Collections.Generic;

using UnityEngine;

public class Vector3Message : OrdenableMessage, IMessage<Vector3>
{
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
    public Vector3 Deserialize(byte[] message)
    {
        Vector3 outData;

        outData.x = BitConverter.ToSingle(message, 12);
        outData.y = BitConverter.ToSingle(message, 16);
        outData.z = BitConverter.ToSingle(message, 20);

        return outData;
    }

    public MESSAGE_TYPE GetMessageType()
    {
        return MESSAGE_TYPE.VECTOR3;
    }

    public byte[] Serialize(float admissionTime)
    {
        List<byte> outData = new List<byte>();

        outData.AddRange(BitConverter.GetBytes((int)GetMessageType()));
        outData.AddRange(BitConverter.GetBytes(admissionTime));
        outData.AddRange(BitConverter.GetBytes(lastMessageId++));

        outData.AddRange(BitConverter.GetBytes(((Vector3)data).x));
        outData.AddRange(BitConverter.GetBytes(((Vector3)data).y));
        outData.AddRange(BitConverter.GetBytes(((Vector3)data).z));

        return outData.ToArray();
    }
    #endregion
}
