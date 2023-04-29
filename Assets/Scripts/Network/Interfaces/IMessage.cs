public enum MESSAGE_TYPE
{
    SERVER_DATA_UPDATE = -6,
    SERVER_ON,
    CONNECT_REQUEST,
    CLIENT_DISCONECT,
    HAND_SHAKE,
    CLIENTS_LIST,
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
