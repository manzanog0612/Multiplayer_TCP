using System;

public static class MessageFormater
{
    #region CONSTANTS
    private const int isReflectionMessageStart = 0;
    private const int sendTimeStart = sizeof(bool);
    private const int messageTypeStart = sendTimeStart + sizeof(int) * 5;
    private const int admissionTimeStart = messageTypeStart + sizeof(int);
    private const int messageIdStart = admissionTimeStart + sizeof(float);
    #endregion

    #region PUBLIC_METHODS
    public static bool IsReflectionMessage(byte[] data)
    {
        return BitConverter.ToBoolean(data, isReflectionMessageStart);
    }

    public static (int days, int hours, int minutes, int seconds, int millisecond) GetMessageSendTime(byte[] data)
    {
        (int days, int hours, int minutes, int seconds, int millisecond) messageSendTime;

        messageSendTime.days = BitConverter.ToInt32(data, sendTimeStart);
        messageSendTime.hours = BitConverter.ToInt32(data, sendTimeStart + sizeof(int));
        messageSendTime.minutes = BitConverter.ToInt32(data, sendTimeStart + sizeof(int) * 2);
        messageSendTime.seconds = BitConverter.ToInt32(data, sendTimeStart + sizeof(int) * 3);
        messageSendTime.millisecond = BitConverter.ToInt32(data, sendTimeStart + sizeof(int) * 4);

        return messageSendTime;
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
