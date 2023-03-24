using System;
using System.Collections.Generic;

public class MessageFormater
{
    #region PUBLIC_METHODS
    public MESSAGE_TYPE GetMessageType(byte[] data)
    {
        List<byte> messageTypeBytes = new List<byte>();

        for (int i = 0; i < 4; i++)
        {
            messageTypeBytes.Add(data[i]);
        }

        MESSAGE_TYPE messageType = (MESSAGE_TYPE)BitConverter.ToInt32(messageTypeBytes.ToArray());

        return messageType;
    }
    #endregion
}
