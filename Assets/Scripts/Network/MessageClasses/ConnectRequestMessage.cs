using System;
using System.Collections.Generic;

public class ConnectRequestMessage : SemiTcpMessage, IMessage<(long, int)>
{
    #region PRIVATE_FIELDS
    private (long, int) data;
    #endregion

    #region CONSTRUCTORS
    public ConnectRequestMessage() { }

    public ConnectRequestMessage((long, int) data)
    {
        this.data = data;
    }
    #endregion

    #region PUBLIC_METHODS
    public static (long, int) Deserialize(byte[] message)
    {
        (long, int) outData;

        int messageStart = GetHeaderSize();

        outData.Item1 = BitConverter.ToInt64(message, messageStart);
        outData.Item2 = BitConverter.ToInt32(message, messageStart + sizeof(long));

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

        messageOperationParts.Add((int)data.Item1);
        messageOperationParts.Add(data.Item2);

        return new MessageTail(messageOperationParts.ToArray(), GetHeaderSize() + GetMessageSize() + GetTailSize());
    }

    public static MESSAGE_TYPE GetMessageType()
    {
        return MESSAGE_TYPE.CONNECT_REQUEST;
    }

    public static int GetMessageSize()
    {
        return sizeof(long) + sizeof(int);
    }

    public byte[] Serialize(float admissionTime = -1)
    {
        List<byte> outData = new List<byte>();

        MessageHeader messageHeader = GetMessageHeader(admissionTime);
        MessageTail messageTail = GetMessageTail();

        outData.AddRange(messageHeader.Bytes);

        outData.AddRange(BitConverter.GetBytes(data.Item1));
        outData.AddRange(BitConverter.GetBytes(data.Item2));

        outData.AddRange(messageTail.Bytes);

        return outData.ToArray();
    }
    #endregion
}
