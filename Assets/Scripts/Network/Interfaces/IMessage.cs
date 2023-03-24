using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MESSAGE_TYPE
{
    STRING,
    VECTOR2
}

public interface IMessage<T>
{
    public MESSAGE_TYPE GetMessageType();
    public byte[] Serialize();
    public T Deserialize(byte[] message);
}
