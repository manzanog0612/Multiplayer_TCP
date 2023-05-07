using System;
using System.Collections.Generic;

public class SyncMessage
{
    #region CONSTRUCTORS
    public SyncMessage() { }
    #endregion

    #region PUBLIC_METHODS
    public MessageHeader GetMessageHeader(float admissionTime)
    {
        return new MessageHeader((int)GetMessageType(), admissionTime);
    }

    public MESSAGE_TYPE GetMessageType()
    {
        return MESSAGE_TYPE.SYNC;
    }

    public byte[] Serialize(float admissionTime)
    {
        List<byte> outData = new List<byte>();

        MessageHeader messageHeader = GetMessageHeader(admissionTime);

        outData.AddRange(messageHeader.Bytes);

        return outData.ToArray();
    }
    #endregion
}
