using System;
using System.Collections.Generic;

public class RemoveEntityMessage : IMessage<int>
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

        outData = BitConverter.ToInt32(message, GetHeaderSize());

        return outData;
    }

    public MessageHeader GetMessageHeader(float admissionTime)
    {
        return new MessageHeader((int)GetMessageType(), admissionTime);
    }

    public MESSAGE_TYPE GetMessageType()
    {
        return MESSAGE_TYPE.ENTITY_DISCONECT;
    }

    public byte[] Serialize(float admissionTime)
    {
        List<byte> outData = new List<byte>();

        MessageHeader messageHeader = GetMessageHeader(admissionTime);

        outData.AddRange(messageHeader.Bytes);
        outData.AddRange(BitConverter.GetBytes(data));

        return outData.ToArray();
    }

    public int GetHeaderSize()
    {
        return sizeof(float) + sizeof(int) + sizeof(float);
    }
    #endregion
}
