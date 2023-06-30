using Game.Common.Requests;
using MultiplayerLibrary;
using MultiplayerLibrary.Interfaces;
using MultiplayerLibrary.Message.Parts;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Common.Networking.Message
{
    public class BulletBornMessage : SemiTcpMessage, IMessage<(int, Vector2, Vector2)>
    {
        #region PRIVATE_FIELDS
        private (int bulletId, Vector2 pos, Vector2 dir) data;
        #endregion

        #region CONSTRUCTORS
        public BulletBornMessage((int, Vector2, Vector2) data)
        {
            this.data = data;
        }
        #endregion

        #region PUBLIC_METHODS
        public static (int, Vector2, Vector2) Deserialize(byte[] message)
        {
            int offset = GetHeaderSize();

            int id = BitConverter.ToInt32(message, offset);
            Vector2 pos = new(BitConverter.ToSingle(message, offset + sizeof(int)),
                              BitConverter.ToSingle(message, offset + sizeof(int) + sizeof(float)));
            Vector2 dir = new(BitConverter.ToSingle(message, offset + sizeof(int) + sizeof(float) * 2),
                              BitConverter.ToSingle(message, offset + sizeof(int) + sizeof(float) * 3));

            (int, Vector2, Vector2) outData = (id, pos, dir);

            return outData;
        }

        public MessageHeader GetMessageHeader(float admissionTime)
        {
            return new MessageHeader((int)GetMessageType(), admissionTime);
        }

        public static int GetHeaderSize()
        {
            return sizeof(bool) + sizeof(int) * MessageHeader.amountIntsInSendTime + sizeof(int) + sizeof(float);
        }

        public override MessageTail GetMessageTail()
        {
            List<int> messageOperationParts = new List<int>();

            messageOperationParts.Add(data.bulletId);
            messageOperationParts.Add((int)(data.pos.x * 100));
            messageOperationParts.Add((int)(data.pos.y * 100));
            messageOperationParts.Add((int)(data.dir.x * 100));
            messageOperationParts.Add((int)(data.dir.y * 100));

            return new MessageTail(messageOperationParts.ToArray(), GetHeaderSize() + GetMessageSize() + GetTailSize());
        }

        public static GAME_MESSAGE_TYPE GetMessageType()
        {
            return GAME_MESSAGE_TYPE.BULLET_BORN;
        }

        public static int GetMessageSize()
        {
            return sizeof(int) + sizeof(float) * 4;
        }

        public byte[] Serialize(float admissionTime = -1)
        {
            List<byte> outData = new List<byte>();

            MessageHeader messageHeader = GetMessageHeader(admissionTime);
            MessageTail messageTail = GetMessageTail();

            outData.AddRange(messageHeader.Bytes);

            outData.AddRange(BitConverter.GetBytes(data.bulletId));
            outData.AddRange(BitConverter.GetBytes(data.pos.x));
            outData.AddRange(BitConverter.GetBytes(data.pos.y));
            outData.AddRange(BitConverter.GetBytes(data.dir.x));
            outData.AddRange(BitConverter.GetBytes(data.dir.y));

            outData.AddRange(messageTail.Bytes);

            return outData.ToArray();
        }
        #endregion
    }
}
