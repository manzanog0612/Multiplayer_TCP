using System;
using System.Collections.Generic;

public class ServerOnMessage : SemiTcpMessage, IMessage<int>
{
    #region PRIVATE_FIELDS
    private int data;
    #endregion

    #region CONSTRUCTORS
    public ServerOnMessage() { }

    public ServerOnMessage(int data)
    {
        this.data = data;
    }
    #endregion

    #region PUBLIC_METHODS
    public static int Deserialize(byte[] message)
    {
        int outData = BitConverter.ToInt32(message, GetHeaderSize());

        return outData;
    }

    public MessageHeader GetMessageHeader(float admissionTime)
    {
        return new MessageHeader((int)GetMessageType());
    }

    public static int GetHeaderSize()
    {
        return sizeof(bool) + sizeof(int) * MessageHeader.amountIntsInSendTime + sizeof(int);
    }

    public override MessageTail GetMessageTail()
    {
        List<int> messageOperationParts = new List<int>();

        messageOperationParts.Add(data);

        return new MessageTail(messageOperationParts.ToArray(), GetHeaderSize() + GetMessageSize() + GetTailSize());
    }

    public static MESSAGE_TYPE GetMessageType()
    {
        return MESSAGE_TYPE.SERVER_ON;
    }

    public static int GetMessageSize()
    {
        return sizeof(int);
    }

    public byte[] Serialize(float admissionTime = -1)
    {
        List<byte> outData = new List<byte>();

        MessageHeader messageHeader = GetMessageHeader(admissionTime);
        MessageTail messageTail = GetMessageTail();

        outData.AddRange(messageHeader.Bytes);

        outData.AddRange(BitConverter.GetBytes(data));
        
        outData.AddRange(messageTail.Bytes);

        return outData.ToArray();
    }
    #endregion
}
