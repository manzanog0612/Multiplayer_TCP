using System;
using System.Collections.Generic;
using Unity.VisualScripting;

public class MessageTail 
{
    public int messageOperationResult = 0;
    public int messageSize = 0;

    public byte[] Bytes { get; private set; }

    public MessageTail(int[] messageParts, int messageSize)
    {
        for (int i = 0; i < messageParts.Length; i++)
        {
            messageOperationResult += messageParts[i];
        }

        this.messageSize = messageSize;

        List<byte> bytes = new List<byte>();

        bytes.AddRange(BitConverter.GetBytes(messageOperationResult));
        bytes.AddRange(BitConverter.GetBytes(messageSize));

        Bytes = bytes.ToArray();
    }

    public MessageTail(int messageOperationResult, int messageSize)
    {
        this.messageOperationResult = messageOperationResult;
        this.messageSize = messageSize;

        List<byte> bytes = new List<byte>();

        bytes.AddRange(BitConverter.GetBytes(messageOperationResult));
        bytes.AddRange(BitConverter.GetBytes(messageSize));

        Bytes = bytes.ToArray();
    }
}
