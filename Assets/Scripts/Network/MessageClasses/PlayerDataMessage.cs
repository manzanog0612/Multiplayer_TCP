using System.Collections.Generic;

using UnityEngine;

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

        MessageFormater messageFormater = new MessageFormater();

        switch (messageFormater.GetMessageType(data))
        {
            case MESSAGE_TYPE.STRING:
                StringMessage stringMessage = new StringMessage();

                string chat = stringMessage.Deserialize(data);
                outData.message = chat;
                break;
            case MESSAGE_TYPE.VECTOR2:
                Vector2Message vector2Message = new Vector2Message();
                outData.movement = outData.position; // a check to know if the player moved
                outData.position = vector2Message.Deserialize(data);
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
            Vector2Message vector2Message = new Vector2Message(data.position);
            outData.AddRange(vector2Message.Serialize(admissionTime));
        }

        return outData.ToArray();
    }
    #endregion
}
