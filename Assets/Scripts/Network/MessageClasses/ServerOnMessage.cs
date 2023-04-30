using System;
using System.Collections.Generic;

public class ServerOnMessage : IMessage<int>
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
    public int Deserialize(byte[] message)
    {
        int outData;

        outData = BitConverter.ToInt32(message, GetHeaderSize());

        return outData;
    }

    public MessageHeader GetMessageHeader(float admissionTime)
    {
        return new MessageHeader((int)GetMessageType());
    }

    public MESSAGE_TYPE GetMessageType()
    {
        return MESSAGE_TYPE.SERVER_ON;
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
        return sizeof(float) + sizeof(int);
    }
    #endregion
}
