public enum MESSAGE_TYPE
{
    SYNC = -8,
    RESEND_DATA,
    SERVER_DATA_UPDATE,
    SERVER_ON,
    CONNECT_REQUEST,
    ENTITY_DISCONECT,
    HAND_SHAKE,
    CLIENTS_LIST,
    STRING = 0,
    VECTOR2,
    VECTOR3
}

public interface IMessage<T>
{
    public MESSAGE_TYPE GetMessageType();

    public T Deserialize(byte[] message);

    public MessageHeader GetMessageHeader(float admissionTime);

    public byte[] Serialize(float admissionTime);

    public int GetHeaderSize();

    public int GetMessageSize();
}
