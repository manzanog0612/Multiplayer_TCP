using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MESSAGE_TYPE
{
    CONNECT_REQUEST = -4,
    CLIENT_DISCONECT = -3,
    HAND_SHAKE = -2,
    CLIENTS_LIST = -1,
    STRING = 0,
    VECTOR2,
    VECTOR3
}

public interface IMessage<T>
{
    public MESSAGE_TYPE GetMessageType();
    public byte[] Serialize(float admissionTime);
    public T Deserialize(byte[] message);
}
