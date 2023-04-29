using System;
using System.Collections.Generic;

public class ServerDataUpdateMessage : IMessage<ServerData>
{
    #region PRIVATE_FIELDS
    private ServerData data = null;
    #endregion

    #region CONSTRUCTORS
    public ServerDataUpdateMessage() { }

    public ServerDataUpdateMessage(ServerData data)
    {
        this.data = data;
    }
    #endregion

    #region PUBLIC_METHODS
    public ServerData Deserialize(byte[] message)
    {
        ServerData outData = new ServerData(BitConverter.ToInt32(message, 4), BitConverter.ToInt32(message, 8), BitConverter.ToInt32(message, 12));

        return outData;
    }

    public MESSAGE_TYPE GetMessageType()
    {
        return MESSAGE_TYPE.SERVER_DATA_UPDATE;
    }

    public byte[] Serialize(float admissionTime)
    {
        List<byte> outData = new List<byte>();

        outData.AddRange(BitConverter.GetBytes((int)GetMessageType()));

        outData.AddRange(BitConverter.GetBytes(data.id));
        outData.AddRange(BitConverter.GetBytes(data.port));
        outData.AddRange(BitConverter.GetBytes(data.amountPlayers));

        return outData.ToArray();
    }
    #endregion
}
