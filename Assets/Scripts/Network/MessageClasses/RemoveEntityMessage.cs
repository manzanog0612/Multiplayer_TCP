using System;
using System.Collections.Generic;

public class RemoveEntityMessage : SemiTcpMessage, IMessage<int>
{
    #region PRIVATE_FIELDS
    private int data;
    #endregion

    #region CONSTRUCTORS
    public RemoveEntityMessage() { }

    public RemoveEntityMessage(int data)
    {
        this.data = data;
    }
    #endregion

    #region PUBLIC_METHODS
    public int Deserialize(byte[] message)
    {
        int outData;

        outData = BitConverter.ToInt32(message, GetHeaderSize() + GetTailSize());

        return outData;
    }

    public MessageHeader GetMessageHeader(float admissionTime)
    {
        return new MessageHeader((int)GetMessageType(), admissionTime);
    }

    public int GetHeaderSize()
    {
        return sizeof(int) * MessageHeader.amountIntsInSendTime + sizeof(int) + sizeof(float);
    }

    public override MessageTail GetMessageTail()
    {
        List<float> messageOperationParts = new List<float>();

        messageOperationParts.Add(data);

        return new MessageTail(messageOperationParts.ToArray());
    }

    public MESSAGE_TYPE GetMessageType()
    {
        return MESSAGE_TYPE.ENTITY_DISCONECT;
    }

    public byte[] Serialize(float admissionTime)
    {
        List<byte> outData = new List<byte>();

        MessageHeader messageHeader = GetMessageHeader(admissionTime);
        MessageTail messageTail = GetMessageTail();

        outData.AddRange(messageHeader.Bytes);
        outData.AddRange(messageTail.Bytes);

        outData.AddRange(BitConverter.GetBytes(data));

        return outData.ToArray();
    }
    #endregion
}
