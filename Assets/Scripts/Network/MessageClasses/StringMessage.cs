using System;
using System.Collections.Generic;

public class StringMessage : IMessage<string>
{
    static public int lastMessageId = 0;

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

        int charSize = sizeof(char);
        int messageStartIndex = GetHeaderSize();

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

    public MessageHeader GetMessageHeader(float admissionTime)
    {
        return new MessageHeader((int)GetMessageType(), admissionTime, lastMessageId);
    }

    public MESSAGE_TYPE GetMessageType()
    {
        return MESSAGE_TYPE.STRING;
    }

    public byte[] Serialize(float admissionTime)
    {
        List<byte> outData = new List<byte>();

        lastMessageId++;
        MessageHeader messageHeader = GetMessageHeader(admissionTime);
        
        outData.AddRange(messageHeader.Bytes);

        for (int i = 0; i < data.Length; i++)
        {
            outData.AddRange(BitConverter.GetBytes(data[i]));
        }

        return outData.ToArray();
    }

    public int GetHeaderSize()
    {
        return sizeof(float) + sizeof(int) + sizeof(float) + sizeof(int);
    }
    #endregion
}
