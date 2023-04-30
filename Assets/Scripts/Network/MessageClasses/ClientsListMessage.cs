using System;
using System.Collections.Generic;

using UnityEngine;

public class ClientsListMessage : IMessage<((int, long, float, Vector3, Color)[], int)>
{
    #region PRIVATE_FIELDS
    private ((int, long, float, Vector3, Color)[] initialDatas, int id) data;
    #endregion

    #region CONSTRUCTORS
    public ClientsListMessage() { }

    public ClientsListMessage(((int, long, float, Vector3, Color)[], int) data)
    {
        this.data = data;
    }
    #endregion

    #region PUBLIC_METHODS
    public ((int, long, float, Vector3, Color)[], int) Deserialize(byte[] message)
    {
        List<(int, long, float, Vector3, Color)> outData = new List<(int, long, float, Vector3, Color)>();

        short messageStartIndex = (short)GetHeaderSize();
        List<byte> bytes = new List<byte>();
        short idBytes = sizeof(int);

        for (int i = messageStartIndex; i < idBytes + messageStartIndex; i++)
        {
            bytes.Add(message[i]);
        }
        
        int id = BitConverter.ToInt32(bytes.ToArray());

        short item1Size = sizeof(int);
        short item2Size = sizeof(long);
        short item3Size = sizeof(float);
        short item4Size = sizeof(float) * 3;
        short item5Size = sizeof(float) * 4;
        short totalItemSize = (short)(item1Size + item2Size + item3Size + item4Size + item5Size);
        messageStartIndex = (short)(GetHeaderSize() + sizeof(int));

        List<byte> GetClientBytes(int i, short size, short itemTotalOffset)
        {
            List<byte> clientBytes = new List<byte>();

            for (int j = 0; j < size; j++)
            {
                clientBytes.Add(message[messageStartIndex + itemTotalOffset + (i * totalItemSize) + j]);
            }

            return clientBytes;
        }

        for (int i = 0; i < (message.Length - messageStartIndex) / totalItemSize; i++)
        {
            List<byte> clientBytes = GetClientBytes(i, item1Size, 0);
            int item1 = BitConverter.ToInt32(clientBytes.ToArray());
            clientBytes.Clear();

            clientBytes = GetClientBytes(i, item2Size, item1Size);
            long item2 = BitConverter.ToInt64(clientBytes.ToArray());
            clientBytes.Clear();

            clientBytes = GetClientBytes(i, item3Size, (short)(item1Size + item2Size));
            float item3 = BitConverter.ToSingle(clientBytes.ToArray());
            clientBytes.Clear();

            clientBytes = GetClientBytes(i, item4Size, (short)(item1Size + item2Size + item3Size));
            Vector3 item4;
            item4.x = BitConverter.ToSingle(clientBytes.ToArray());
            item4.y = BitConverter.ToSingle(clientBytes.ToArray(), sizeof(float));
            item4.z = BitConverter.ToSingle(clientBytes.ToArray(), sizeof(float) * 2);
            clientBytes.Clear();

            clientBytes = GetClientBytes(i, item5Size, (short)(item1Size + item2Size + item3Size + item4Size));
            Color item5;
            item5.r = BitConverter.ToSingle(clientBytes.ToArray());
            item5.g = BitConverter.ToSingle(clientBytes.ToArray(), sizeof(float));
            item5.b = BitConverter.ToSingle(clientBytes.ToArray(), sizeof(float) * 2);
            item5.a = BitConverter.ToSingle(clientBytes.ToArray(), sizeof(float) * 3);

            outData.Add((item1, item2, item3, item4, item5));
        }

        return (outData.ToArray(), id);
    }

    public MessageHeader GetMessageHeader(float admissionTime)
    {
        return new MessageHeader((int)GetMessageType());
    }

    public MESSAGE_TYPE GetMessageType()
    {
        return MESSAGE_TYPE.CLIENTS_LIST;
    }

    public byte[] Serialize(float admissionTime)
    {
        List<byte> outData = new List<byte>();

        MessageHeader messageHeader = GetMessageHeader(admissionTime);

        outData.AddRange(messageHeader.Bytes);
        outData.AddRange(BitConverter.GetBytes(data.id));

        for (int i = 0; i < data.initialDatas.Length; i++)
        {
            outData.AddRange(BitConverter.GetBytes(data.initialDatas[i].Item1));
            outData.AddRange(BitConverter.GetBytes(data.initialDatas[i].Item2));
            outData.AddRange(BitConverter.GetBytes(data.initialDatas[i].Item3));
            outData.AddRange(BitConverter.GetBytes(data.initialDatas[i].Item4.x));
            outData.AddRange(BitConverter.GetBytes(data.initialDatas[i].Item4.y));
            outData.AddRange(BitConverter.GetBytes(data.initialDatas[i].Item4.z));
            outData.AddRange(BitConverter.GetBytes(data.initialDatas[i].Item5.r));
            outData.AddRange(BitConverter.GetBytes(data.initialDatas[i].Item5.g));
            outData.AddRange(BitConverter.GetBytes(data.initialDatas[i].Item5.b));
            outData.AddRange(BitConverter.GetBytes(data.initialDatas[i].Item5.a));
        }

        return outData.ToArray();
    }

    public int GetHeaderSize()
    {
        return sizeof(float) + sizeof(int);
    }
    #endregion
}
