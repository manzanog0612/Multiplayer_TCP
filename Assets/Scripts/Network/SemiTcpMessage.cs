using System;

public abstract class SemiTcpMessage
{
    public abstract MessageTail GetMessageTail();

    public int GetTailSize()
    {
        return sizeof(int) + sizeof(int);
    }

    public MessageTail DeserializeTail(byte[] message, int headerSize, int messageSize)
    {
        return new MessageTail(BitConverter.ToInt32(message, headerSize + messageSize), BitConverter.ToInt32(message, headerSize + messageSize + sizeof(int)));
    }
}
