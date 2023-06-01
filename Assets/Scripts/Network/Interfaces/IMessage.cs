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
    public static MESSAGE_TYPE GetMessageType() { return 0; }

    public static T Deserialize(byte[] message) { return default(T); }

    public MessageHeader GetMessageHeader(float admissionTime);

    public byte[] Serialize(float admissionTime = -1);

    public static int GetHeaderSize() { return 0; }

    public static int GetMessageSize() { return 0; }
}
