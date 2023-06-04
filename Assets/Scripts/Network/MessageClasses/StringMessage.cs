using System;
using System.Collections.Generic;

public class StringMessage : SemiTcpMessage, IMessage<string>
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
    public static string Deserialize(byte[] message)
    {
        string outData = string.Empty;

        int charSize = sizeof(char);
        int messageStartIndex = GetHeaderSize();

        for (int i = 0; i < (message.Length - messageStartIndex - GetTailSize()) / charSize; i++)
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
        return new MessageHeader((int)GetMessageType(), admissionTime);
    }

    public static int GetHeaderSize()
    {
        return sizeof(bool) + sizeof(int) * MessageHeader.amountIntsInSendTime + sizeof(int) + sizeof(float);
    }

    public override MessageTail GetMessageTail()
    {
        List<int> messageOperationParts = new List<int>();

        for (int i = 0; i < data.Length; i++)
        {
            messageOperationParts.Add(data[i]);
        }

        return new MessageTail(messageOperationParts.ToArray(), GetHeaderSize() + GetMessageSize() + GetTailSize());
    }

    public static MESSAGE_TYPE GetMessageType()
    {
        return MESSAGE_TYPE.STRING;
    }

    public int GetMessageSize()
    {
        return sizeof(char) * data.Length;
    }

    public byte[] Serialize(float admissionTime)
    {
        List<byte> outData = new List<byte>();

        MessageHeader messageHeader = GetMessageHeader(admissionTime);
        MessageTail messageTail = GetMessageTail();

        outData.AddRange(messageHeader.Bytes);

        for (int i = 0; i < data.Length; i++)
        {
            outData.AddRange(BitConverter.GetBytes(data[i]));
        }
        
        outData.AddRange(messageTail.Bytes);

        return outData.ToArray();
    }
    #endregion
}
