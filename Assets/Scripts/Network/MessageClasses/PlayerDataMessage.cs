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
                StringMessage stringMessage = new StringMessage();

                string chat = stringMessage.Deserialize(data);
                outData.message = chat;
                break;
            case MESSAGE_TYPE.VECTOR3:
                Vector3Message vector3Message = new Vector3Message();
                outData.movement = outData.position; // a check to know if the player moved
                outData.position = vector3Message.Deserialize(data);
                break;
            default:
                break;
        }

        return outData;
    }

    public MESSAGE_TYPE GetMessageType()
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

    int IMessage<PlayerData>.GetHeaderSize()
    {
        throw new System.NotImplementedException();
    }

    MessageHeader IMessage<PlayerData>.GetMessageHeader(float admissionTime)
    {
        throw new System.NotImplementedException();
    }
    #endregion
}
