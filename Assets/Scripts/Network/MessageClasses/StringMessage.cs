using System;
using System.Collections.Generic;

public class StringMessage : IMessage<string>
{
    #region PRIVATE_FIELDS
    private string data = null;
    #endregion

    #region CONSTRUCTORS
    public StringMessage() { }

    public StringMessage(string data)
    {
        this.data = data;   
    }
    #endregion

    #region PUBLIC_METHODS
    public string Deserialize(byte[] message)
    {
        string outData = string.Empty;

        for (int i = 0; i < (message.Length - 4) / 2 ; i++)
        {
            List<byte> charBytes = new List<byte>();

            for (int j = 0; j < 2; j++)
            {
                charBytes.Add(message[4 + (i * 2) + j]);
            }

            outData += BitConverter.ToChar(charBytes.ToArray());
        }

        return outData;
    }

    public MESSAGE_TYPE GetMessageType()
    {
        return MESSAGE_TYPE.STRING;
    }

    public byte[] Serialize()
    {
        List<byte> outData = new List<byte>();

        outData.AddRange(BitConverter.GetBytes((int)GetMessageType()));

        for (int i = 0; i < data.Length; i++)
        {
            outData.AddRange(BitConverter.GetBytes(data[i]));
        }

        return outData.ToArray();
    }
    #endregion
}
