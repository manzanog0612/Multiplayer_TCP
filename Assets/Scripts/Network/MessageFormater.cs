using System;

public static class MessageFormater
{
    #region CONSTANTS
    private const int sendTimeStart = 0;
    private const int messageTypeStart = sizeof(float);
    private const int admissionTimeStart = messageTypeStart + sizeof(int);
    private const int messageIdStart = admissionTimeStart + sizeof(float);
    #endregion

    #region PUBLIC_METHODS
    public static float GetMessageSendTime(byte[] data)
    {
        float messageType = BitConverter.ToSingle(data, sendTimeStart);

        return messageType;
    }


    public static MESSAGE_TYPE GetMessageType(byte[] data)
    {
        MESSAGE_TYPE messageType = (MESSAGE_TYPE)BitConverter.ToInt32(data, messageTypeStart);

        return messageType;
    }

    public static float GetAdmissionTime(byte[] data)
    {
        float messageId = BitConverter.ToSingle(data, admissionTimeStart);

        return messageId;
    }

    public static int GetMessageId(byte[] data)
    {
        int messageId = BitConverter.ToInt32(data, messageIdStart);

        return messageId;
    }
    #endregion
}
