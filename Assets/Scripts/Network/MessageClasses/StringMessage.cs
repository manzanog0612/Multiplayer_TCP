using System;
using System.Collections.Generic;
using Unity.VisualScripting;

public class StringMessage : IMessage<string>
{
    private string data;

    public StringMessage(string data)
    {
        this.data = data;   
    }

    public StringMessage()
    {

    }

    public string Deserialize(byte[] message)
    {
        string outdata = string.Empty;

        for (int i = 0; i < (message.Length - 4) / 2 ; i++)
        {
            List<byte> charBytes = new List<byte>();

            for (int j = 0; j < 2; j++)
            {
                charBytes.Add(message[4 + (i * 2) + j]);
            }

            outdata += BitConverter.ToChar(charBytes.ToArray());
        }

        return outdata;
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
}
