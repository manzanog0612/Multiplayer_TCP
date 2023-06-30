using Game.Common.Requests;
using MultiplayerLibrary.Interfaces;
using MultiplayerLibrary.Message.Parts;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Common.Networking.Message
{
    public class PlayerPositionMessage : IMessage<(int, Vector2)>
    {
        static public int lastMessageId = 0;

        #region PRIVATE_FIELDS
        private (int id, Vector2 pos) data;
        #endregion

        #region CONSTRUCTORS
        public PlayerPositionMessage((int, Vector2) data)
        {
            this.data = data;
        }
        #endregion

        #region PUBLIC_METHODS
        public static (int, Vector2) Deserialize(byte[] message)
        {
            (int id, Vector2 pos) outData;

            int headerSize = GetHeaderSize();

            outData.id = BitConverter.ToInt32(message, headerSize);
            outData.pos = new Vector2(BitConverter.ToSingle(message, headerSize + sizeof(int)),
                                      BitConverter.ToSingle(message, headerSize + sizeof(int) + sizeof(float)));

            return outData;
        }

        public MessageHeader GetMessageHeader(float admissionTime)
        {
            return new MessageHeader((int)GetMessageType(), lastMessageId);
        }

        public static GAME_MESSAGE_TYPE GetMessageType()
        {
            return GAME_MESSAGE_TYPE.PLAYER_POS;
        }

        public byte[] Serialize(float admissionTime = -1)
        {
            List<byte> outData = new List<byte>();

            lastMessageId++;
            MessageHeader messageHeader = GetMessageHeader(admissionTime);

            outData.AddRange(messageHeader.Bytes);
            outData.AddRange(BitConverter.GetBytes(data.id));
            outData.AddRange(BitConverter.GetBytes(data.pos.x));
            outData.AddRange(BitConverter.GetBytes(data.pos.y));

            return outData.ToArray();
        }

        public static int GetHeaderSize()
        {
            return sizeof(bool) + sizeof(int) * MessageHeader.amountIntsInSendTime + sizeof(int) + sizeof(int);
        }

        public static int GetMessageSize()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
