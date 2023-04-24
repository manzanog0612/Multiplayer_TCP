using System;
using System.Collections.Generic;

public static class MessageFormater
{
    #region PUBLIC_METHODS
    public static MESSAGE_TYPE GetMessageType(byte[] data)
    {
        List<byte> messageTypeBytes = new List<byte>();

        for (int i = 0; i < 4; i++)
        {
            messageTypeBytes.Add(data[i]);
        }

        MESSAGE_TYPE messageType = (MESSAGE_TYPE)BitConverter.ToInt32(messageTypeBytes.ToArray());

        return messageType;
    }

    public static int GetMessageId(byte[] data)
    {
        List<byte> messageTypeBytes = new List<byte>();

        for (int i = 8; i < 12; i++)
        {
            messageTypeBytes.Add(data[i]);
        }

        int messageId = BitConverter.ToInt32(messageTypeBytes.ToArray());

        return messageId;
    }

    public static float GetAdmissionTime(byte[] data)
    {
        List<byte> messageTypeBytes = new List<byte>();

        for (int i = 4; i < 8; i++)
        {
            messageTypeBytes.Add(data[i]);
        }

        float messageId = BitConverter.ToSingle(messageTypeBytes.ToArray());

        return messageId;
    }
    #endregion
}
