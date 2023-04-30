using System;
using System.Collections.Generic;

public class ServerDataUpdateMessage : IMessage<ServerData>
{
    #region PRIVATE_FIELDS
    private ServerData data = null;
    #endregion

    #region CONSTRUCTORS
    public ServerDataUpdateMessage() { }

    public ServerDataUpdateMessage(ServerData data)
    {
        this.data = data;
    }
    #endregion

    #region PUBLIC_METHODS
    public ServerData Deserialize(byte[] message)
    {
        ServerData outData = new ServerData(BitConverter.ToInt32(message, GetHeaderSize()), BitConverter.ToInt32(message, GetHeaderSize() + sizeof(int)), BitConverter.ToInt32(message, GetHeaderSize() + sizeof(int) + sizeof(int)));

        return outData;
    }

    public MessageHeader GetMessageHeader(float admissionTime)
    {
        return new MessageHeader((int)GetMessageType());
    }

    public MESSAGE_TYPE GetMessageType()
    {
        return MESSAGE_TYPE.SERVER_DATA_UPDATE;
    }

    public byte[] Serialize(float admissionTime)
    {
        List<byte> outData = new List<byte>();

        MessageHeader messageHeader = GetMessageHeader(admissionTime);

        outData.AddRange(messageHeader.Bytes);
        outData.AddRange(BitConverter.GetBytes(data.id));
        outData.AddRange(BitConverter.GetBytes(data.port));
        outData.AddRange(BitConverter.GetBytes(data.amountPlayers));

        return outData.ToArray();
    }

    public int GetHeaderSize()
    {
        return sizeof(float) + sizeof(int);
    }
    #endregion
}
