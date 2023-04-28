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

        outData = BitConverter.ToInt32(message, 4);

        return outData;
    }

    public MESSAGE_TYPE GetMessageType()
    {
        return MESSAGE_TYPE.SERVER_ON;
    }

    public byte[] Serialize(float admissionTime)
    {
        List<byte> outData = new List<byte>();

        outData.AddRange(BitConverter.GetBytes((int)GetMessageType()));
        outData.AddRange(BitConverter.GetBytes(data));

        return outData.ToArray();
    }
    #endregion
}
