using System.Collections.Generic;

public class PlayerDataMessage : IMessage<PlayerData>
{
    #region PRIVATE_FIELDS
    private PlayerData data = null;
    #endregion

    #region CONSTRUCTORS
    public PlayerDataMessage(int id) 
    {
        data = new PlayerData();
        data.id = id;
    }

    public PlayerDataMessage(PlayerData data)
    {
        this.data = data;
    }
    #endregion

    #region PUBLIC_METHODS
    public PlayerData Deserialize(byte[] data)
    {
        PlayerData outData = this.data;

        switch (MessageFormater.GetMessageType(data))
        {
            case MESSAGE_TYPE.STRING:
                string chat = StringMessage.Deserialize(data);
                outData.message = chat;
                break;
            case MESSAGE_TYPE.VECTOR3:
                outData.movement = outData.position; // a check to know if the player moved
                outData.position = Vector3Message.Deserialize(data);
                break;
            default:
                break;
        }

        return outData;
    }

    public static int GetMessageSize()
    {
        throw new System.NotImplementedException();
    }

    public static MESSAGE_TYPE GetMessageType()
    {
        throw new System.NotImplementedException();
    }

    public byte[] Serialize(float admissionTime)
    {
        List<byte> outData = new List<byte>();

        if (data.message != null)
        {
            StringMessage stringMessage = new StringMessage(data.message);
            outData.AddRange(stringMessage.Serialize(admissionTime));
        }

        if (data.movement != null)
        {
            Vector3Message vector2Message = new Vector3Message(data.position);
            outData.AddRange(vector2Message.Serialize(admissionTime));
        }

        return outData.ToArray();
    }

    static int GetHeaderSize()
    {
        return 0;
    }

    MessageHeader IMessage<PlayerData>.GetMessageHeader(float admissionTime)
    {
        throw new System.NotImplementedException();
    }
    #endregion
}
