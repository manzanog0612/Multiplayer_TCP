using System;

using System.Collections.Generic;
using UnityEngine;

public class ClientsListMessage : IMessage<(long, float, Vector2, Color)[]>
{
    #region PRIVATE_FIELDS
    private (long, float, Vector2, Color)[] data;
    #endregion

    #region CONSTRUCTORS
    public ClientsListMessage() { }

    public ClientsListMessage((long, float, Vector2, Color)[] data)
    {
        this.data = data;
    }
    #endregion

    #region PUBLIC_METHODS
    public (long, float, Vector2, Color)[] Deserialize(byte[] message)
    {
        List<(long, float, Vector2, Color)> outData = new List<(long, float, Vector2, Color)>();

        short item1Size = 8;// long 8
        short item2Size = 4;// float 4
        short item3Size = 8; // float 4 * 2 (vector2)
        short item4Size = 16;
        short totalItemSize = (short)(item1Size + item2Size + item3Size + item4Size);
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

            clientBytes.Clear();

            for (int j = 0; j < item3Size; j++)
            {
                clientBytes.Add(message[messageStartIndex + item1Size + item2Size + (i * totalItemSize) + j]);
            }

            Vector2 item3;

            item3.x = BitConverter.ToSingle(clientBytes.ToArray(), 0);
            item3.y = BitConverter.ToSingle(clientBytes.ToArray(), 4);

            clientBytes.Clear();

            for (int j = 0; j < item4Size; j++)
            {
                clientBytes.Add(message[messageStartIndex + item1Size + item2Size + item3Size +(i * totalItemSize) + j]);
            }

            Color item4;

            item4.r = BitConverter.ToSingle(clientBytes.ToArray(), 0);
            item4.g = BitConverter.ToSingle(clientBytes.ToArray(), 4);
            item4.b = BitConverter.ToSingle(clientBytes.ToArray(), 8);
            item4.a = BitConverter.ToSingle(clientBytes.ToArray(), 12);

            outData.Add((item1, item2, item3, item4));
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
            outData.AddRange(BitConverter.GetBytes(data[i].Item3.x));
            outData.AddRange(BitConverter.GetBytes(data[i].Item3.y));
            outData.AddRange(BitConverter.GetBytes(data[i].Item4.r));
            outData.AddRange(BitConverter.GetBytes(data[i].Item4.g));
            outData.AddRange(BitConverter.GetBytes(data[i].Item4.b));
            outData.AddRange(BitConverter.GetBytes(data[i].Item4.a));
        }

        return outData.ToArray();
    }
    #endregion
}
