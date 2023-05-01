using System;

public class MessageTail 
{
    public float messageOperationResult = 0;

    public byte[] Bytes => BitConverter.GetBytes(messageOperationResult);

    public MessageTail(float[] messageParts)
    {
        for (int i = 0; i < messageParts.Length; i++)
        {
            messageOperationResult += messageParts[i];
        }
    }

    public MessageTail(float messageOperationResult)
    {
        this.messageOperationResult = messageOperationResult;   
    }
}
