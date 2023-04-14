using System;
using System.Collections.Generic;

public class StringMessage : OrdenableMessage, IMessage<string>
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

        short charSize = 2;
        short messageStartIndex = 12;

        for (int i = 0; i < (message.Length - messageStartIndex) / charSize; i++)
        {
            List<byte> charBytes = new List<byte>();

            for (int j = 0; j < charSize; j++)
            {
                charBytes.Add(message[messageStartIndex + (i * charSize) + j]);
            }

            outData += BitConverter.ToChar(charBytes.ToArray());
        }

        return outData; //+ " " + new MessageFormater().GetMessageId(message);
    }

    public MESSAGE_TYPE GetMessageType()
    {
        return MESSAGE_TYPE.STRING;
    }

    public byte[] Serialize(float admissionTime)
    {
        List<byte> outData = new List<byte>();

        outData.AddRange(BitConverter.GetBytes((int)GetMessageType()));
        outData.AddRange(BitConverter.GetBytes(admissionTime));
        outData.AddRange(BitConverter.GetBytes(lastMessageId++));

        for (int i = 0; i < data.Length; i++)
        {
            outData.AddRange(BitConverter.GetBytes(data[i]));
        }

        return outData.ToArray();
    }
    #endregion
}
