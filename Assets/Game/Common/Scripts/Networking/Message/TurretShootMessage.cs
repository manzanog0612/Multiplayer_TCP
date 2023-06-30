using Game.Common.Requests;
using MultiplayerLibrary;
using MultiplayerLibrary.Interfaces;
using MultiplayerLibrary.Message.Parts;
using System;
using System.Collections.Generic;

namespace Game.Common.Networking.Message
{
    public class TurretShootMessage : SemiTcpMessage, IMessage<(int, int)>
    {
        #region PRIVATE_FIELDS
        private (int turretId, int bulletId) data;
        #endregion

        #region CONSTRUCTORS
        public TurretShootMessage((int, int) data)
        {
            this.data = data;
        }
        #endregion

        #region PUBLIC_METHODS
        public static (int, int) Deserialize(byte[] message)
        {
            int offset = GetHeaderSize();

            (int turretId, int bulletId) outData = new (BitConverter.ToInt32(message, offset), 
                                                        BitConverter.ToInt32(message, offset + sizeof(int)));

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

            messageOperationParts.Add(data.turretId);
            messageOperationParts.Add(data.bulletId);

            return new MessageTail(messageOperationParts.ToArray(), GetHeaderSize() + GetMessageSize() + GetTailSize());
        }

        public static GAME_MESSAGE_TYPE GetMessageType()
        {
            return GAME_MESSAGE_TYPE.TURRET_SHOOT;
        }

        public static int GetMessageSize()
        {
            return sizeof(int) * 2;
        }

        public byte[] Serialize(float admissionTime = -1)
        {
            List<byte> outData = new List<byte>();

            MessageHeader messageHeader = GetMessageHeader(admissionTime);
            MessageTail messageTail = GetMessageTail();

            outData.AddRange(messageHeader.Bytes);

            outData.AddRange(BitConverter.GetBytes(data.turretId));
            outData.AddRange(BitConverter.GetBytes(data.bulletId));

            outData.AddRange(messageTail.Bytes);

            return outData.ToArray();
        }
        #endregion
    }
}
