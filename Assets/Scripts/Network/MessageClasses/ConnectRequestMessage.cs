using System;
using System.Collections.Generic;

public class ConnectRequestMessage : IMessage<(long, int)>
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
    public (long, int) Deserialize(byte[] message)
    {
        (long, int) outData;

        outData.Item1 = BitConverter.ToInt64(message, 4);
        outData.Item2 = BitConverter.ToInt32(message, 12);

        return outData;
    }

    public MESSAGE_TYPE GetMessageType()
    {
        return MESSAGE_TYPE.CONNECT_REQUEST;
    }

    public byte[] Serialize(float admissionTime)
    {
        List<byte> outData = new List<byte>();

        outData.AddRange(BitConverter.GetBytes((int)GetMessageType()));
        outData.AddRange(BitConverter.GetBytes(data.Item1));
        outData.AddRange(BitConverter.GetBytes(data.Item2));

        return outData.ToArray();
    }
    #endregion
}
