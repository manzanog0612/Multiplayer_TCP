using System;

using System.Collections.Generic;

public class ClientsListMessage : IMessage<(long, float)[]>
{
    #region PRIVATE_FIELDS
    private (long, float)[] data;
    #endregion

    #region CONSTRUCTORS
    public ClientsListMessage() { }

    public ClientsListMessage((long, float)[] data)
    {
        this.data = data;
    }
    #endregion

    #region PUBLIC_METHODS
    public (long, float)[] Deserialize(byte[] message)
    {
        List<(long, float)> outData = new List<(long, float)>();

        short item1Size = 8;// long 8
        short item2Size = 4;// float 4
        short totalItemSize = (short)(item1Size + item2Size);
        short messageStartIndex = 4;

        for (int i = 0; i < (message.Length - messageStartIndex) / totalItemSize; i++) 
        {
            List<byte> clientBytes = new List<byte>(); 

            for (int j = 0; j < item1Size; j++)
            {
                clientBytes.Add(message[messageStartIndex + (i * totalItemSize) + j]);
            }

            long item1 = BitConverter.ToInt64(clientBytes.ToArray());

            clientBytes.Clear();

            for (int j = 0; j < item2Size; j++)
            {
                clientBytes.Add(message[messageStartIndex + item1Size + (i * totalItemSize) + j]);
            }

            float item2 = BitConverter.ToSingle(clientBytes.ToArray());

            outData.Add((item1, item2));
        }

        return outData.ToArray();
    }

    public MESSAGE_TYPE GetMessageType()
    {
        return MESSAGE_TYPE.CLIENTS_LIST;
    }

    public byte[] Serialize(float admissionTime)
    {
        List<byte> outData = new List<byte>();

        outData.AddRange(BitConverter.GetBytes((int)GetMessageType()));

        for (int i = 0; i < data.Length; i++)
        {
            outData.AddRange(BitConverter.GetBytes(data[i].Item1));
            outData.AddRange(BitConverter.GetBytes(data[i].Item2));
        }

        return outData.ToArray();
    }
    #endregion
}
