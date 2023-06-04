using System;
using System.Collections.Generic;

using UnityEngine;

public class HandShakeMessage : SemiTcpMessage, IMessage<(long, int, Color)>
{
    #region PRIVATE_FIELDS
    private (long, int, Color) data;
    #endregion

    #region CONSTRUCTORS
    public HandShakeMessage() { }

    public HandShakeMessage((long, int, Color) data)
    {
        this.data = data;
    }
    #endregion

    #region PUBLIC_METHODS
    public static (long, int, Color) Deserialize(byte[] message)
    {
        (long, int, Color) outData;

        int messageStart = GetHeaderSize();

        outData.Item1 = BitConverter.ToInt64(message, messageStart);
        outData.Item2 = BitConverter.ToInt32(message, messageStart + sizeof(long));
        outData.Item3.r = BitConverter.ToSingle(message, messageStart + sizeof(long) + sizeof(int));
        outData.Item3.g = BitConverter.ToSingle(message, messageStart + sizeof(long) + sizeof(int) + sizeof(float));
        outData.Item3.b = BitConverter.ToSingle(message, messageStart + sizeof(long) + sizeof(int) + sizeof(float) * 2);
        outData.Item3.a = BitConverter.ToSingle(message, messageStart + sizeof(long) + sizeof(int) + sizeof(float) * 3);

        return outData;
    }

    public MessageHeader GetMessageHeader(float admissionTime)
    {
        return new MessageHeader((int)GetMessageType(), admissionTime);
    }

    public static MESSAGE_TYPE GetMessageType()
    {
        return MESSAGE_TYPE.HAND_SHAKE;
    }

    public static int GetHeaderSize()
    {
        return sizeof(bool) + sizeof(int) * MessageHeader.amountIntsInSendTime + sizeof(int) + sizeof(float);
    }

    public static int GetMessageSize()
    {
        // (long, int, Color)
        return sizeof(long) + sizeof(int) + sizeof(float) * 4;
    }

    public override MessageTail GetMessageTail()
    {
        List<int> messageOperationParts = new List<int>();

        messageOperationParts.Add((int)data.Item1);
        messageOperationParts.Add(data.Item2);
        messageOperationParts.Add((int)(data.Item3.r * 100));
        messageOperationParts.Add((int)(data.Item3.g * 100));
        messageOperationParts.Add((int)(data.Item3.b * 100));
        messageOperationParts.Add((int)(data.Item3.a * 100));

        return new MessageTail(messageOperationParts.ToArray(), GetHeaderSize() + GetMessageSize() + GetTailSize());
    }

    public byte[] Serialize(float admissionTime)
    {
        List<byte> outData = new List<byte>();

        MessageHeader messageHeader = GetMessageHeader(admissionTime);
        MessageTail messageTail = GetMessageTail();

        outData.AddRange(messageHeader.Bytes);

        outData.AddRange(BitConverter.GetBytes(data.Item1));
        outData.AddRange(BitConverter.GetBytes(data.Item2));
        outData.AddRange(BitConverter.GetBytes(data.Item3.r));
        outData.AddRange(BitConverter.GetBytes(data.Item3.g));
        outData.AddRange(BitConverter.GetBytes(data.Item3.b));
        outData.AddRange(BitConverter.GetBytes(data.Item3.a));
        
        outData.AddRange(messageTail.Bytes);

        return outData.ToArray();
    }
    #endregion
}
