using System;

public abstract class SemiTcpMessage
{
    public abstract MessageTail GetMessageTail();

    public int GetTailSize()
    {
        return sizeof(float);
    }

    public MessageTail DeserializeTail(byte[] message, int headerSize)
    {
        return new MessageTail(BitConverter.ToSingle(message, headerSize));
    }
}
