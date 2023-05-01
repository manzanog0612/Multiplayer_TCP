using System;
using System.Collections.Generic;

public class ResendDataMessage : SemiTcpMessage, IMessage<MESSAGE_TYPE>
{
    #region PRIVATE_FIELDS
    private MESSAGE_TYPE data;
    #endregion

    #region CONSTRUCTORS
    public ResendDataMessage() { }

    public ResendDataMessage(MESSAGE_TYPE data)
    {
        this.data = data;
    }
    #endregion

    #region PUBLIC_METHODS
    public MESSAGE_TYPE Deserialize(byte[] message)
    {
        MESSAGE_TYPE outData = (MESSAGE_TYPE)BitConverter.ToInt32(message, GetHeaderSize() + GetTailSize());

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

        messageOperationParts.Add((int)data);

        return new MessageTail(messageOperationParts.ToArray());
    }

    public MESSAGE_TYPE GetMessageType()
    {
        return MESSAGE_TYPE.RESEND_DATA;
    }

    public byte[] Serialize(float admissionTime)
    {
        List<byte> outData = new List<byte>();

        MessageHeader messageHeader = GetMessageHeader(admissionTime);
        MessageTail messageTail = GetMessageTail();

        outData.AddRange(messageHeader.Bytes);
        outData.AddRange(messageTail.Bytes);

        outData.AddRange(BitConverter.GetBytes((int)data));

        return outData.ToArray();
    }
    #endregion
}