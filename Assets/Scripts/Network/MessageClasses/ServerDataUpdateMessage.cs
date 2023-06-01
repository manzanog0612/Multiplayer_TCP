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
    public static ServerData Deserialize(byte[] message)
    {
        int messageStart = GetHeaderSize();

        ServerData outData = new ServerData(BitConverter.ToInt32(message, messageStart), BitConverter.ToInt32(message, messageStart + sizeof(int)), BitConverter.ToInt32(message, messageStart + sizeof(int) + sizeof(int)));

        return outData;
    }

    public MessageHeader GetMessageHeader(float admissionTime)
    {
        return new MessageHeader((int)GetMessageType());
    }

    public static int GetHeaderSize()
    {
        return sizeof(int) * MessageHeader.amountIntsInSendTime + sizeof(int);
    }

    public override MessageTail GetMessageTail()
    {
        List<int> messageOperationParts = new List<int>();

        messageOperationParts.Add(data.id);
        messageOperationParts.Add(data.port);
        messageOperationParts.Add(data.amountPlayers);

        return new MessageTail(messageOperationParts.ToArray(), GetHeaderSize() + GetMessageSize() + GetTailSize());
    }

    public static MESSAGE_TYPE GetMessageType()
    {
        return MESSAGE_TYPE.SERVER_DATA_UPDATE;
    }

    public static int GetMessageSize()
    {
        return sizeof(int) * 3;
    }

    public byte[] Serialize(float admissionTime = -1)
    {
        List<byte> outData = new List<byte>();

        MessageHeader messageHeader = GetMessageHeader(admissionTime);
        MessageTail messageTail = GetMessageTail();

        outData.AddRange(messageHeader.Bytes);

        outData.AddRange(BitConverter.GetBytes(data.id));
        outData.AddRange(BitConverter.GetBytes(data.port));
        outData.AddRange(BitConverter.GetBytes(data.amountPlayers));
        
        outData.AddRange(messageTail.Bytes);

        return outData.ToArray();
    }
    #endregion
}
