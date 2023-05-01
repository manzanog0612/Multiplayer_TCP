using System;
using System.Collections.Generic;

public class ServerDataUpdateMessage : SemiTcpMessage, IMessage<ServerData>
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
        int messageStart = GetHeaderSize() + GetTailSize();

        ServerData outData = new ServerData(BitConverter.ToInt32(message, messageStart), BitConverter.ToInt32(message, messageStart + sizeof(int)), BitConverter.ToInt32(message, messageStart + sizeof(int) + sizeof(int)));

        return outData;
    }

    public MessageHeader GetMessageHeader(float admissionTime)
    {
        return new MessageHeader((int)GetMessageType());
    }

    public int GetHeaderSize()
    {
        return sizeof(int) * MessageHeader.amountIntsInSendTime + sizeof(int);
    }

    public override MessageTail GetMessageTail()
    {
        List<float> messageOperationParts = new List<float>();

        messageOperationParts.Add(data.id);
        messageOperationParts.Add(data.port);
        messageOperationParts.Add(data.amountPlayers);

        return new MessageTail(messageOperationParts.ToArray());
    }

    public MESSAGE_TYPE GetMessageType()
    {
        return MESSAGE_TYPE.SERVER_DATA_UPDATE;
    }

    public byte[] Serialize(float admissionTime)
    {
        List<byte> outData = new List<byte>();

        MessageHeader messageHeader = GetMessageHeader(admissionTime);
        MessageTail messageTail = GetMessageTail();

        outData.AddRange(messageHeader.Bytes);
        outData.AddRange(messageTail.Bytes);

        outData.AddRange(BitConverter.GetBytes(data.id));
        outData.AddRange(BitConverter.GetBytes(data.port));
        outData.AddRange(BitConverter.GetBytes(data.amountPlayers));

        return outData.ToArray();
    }
    #endregion
}
