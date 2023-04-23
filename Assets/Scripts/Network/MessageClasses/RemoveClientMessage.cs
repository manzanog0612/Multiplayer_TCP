using System;
using System.Collections.Generic;

public class RemoveClientMessage : IMessage<int>
{
    #region PRIVATE_FIELDS
    private int data;
    #endregion

    #region CONSTRUCTORS
    public RemoveClientMessage() { }

    public RemoveClientMessage(int data)
    {
        this.data = data;
    }
    #endregion

    #region PUBLIC_METHODS
    public int Deserialize(byte[] message)
    {
        int outData;

        outData = BitConverter.ToInt32(message, 8);

        return outData;
    }

    public MESSAGE_TYPE GetMessageType()
    {
        return MESSAGE_TYPE.CLIENT_DISCONECT;
    }

    public byte[] Serialize(float admissionTime)
    {
        List<byte> outData = new List<byte>();

        outData.AddRange(BitConverter.GetBytes((int)GetMessageType()));
        outData.AddRange(BitConverter.GetBytes(admissionTime));
        outData.AddRange(BitConverter.GetBytes(data));

        return outData.ToArray();
    }
    #endregion
}
