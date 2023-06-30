using Game.Common.Requests;
using MultiplayerLibrary.Interfaces;
using MultiplayerLibrary.Message.Parts;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Common.Networking.Message
{
    public class TurretRotationMessage : IMessage<(int, Quaternion)>
    {
        static public int lastMessageId = 0;

        #region PRIVATE_FIELDS
        private (int turretId, Quaternion rotation) data;
        #endregion

        #region CONSTRUCTORS
        public TurretRotationMessage((int, Quaternion) data)
        {
            this.data = data;
        }
        #endregion

        #region PUBLIC_METHODS
        public static (int, Quaternion) Deserialize(byte[] message)
        {
            int offset = GetHeaderSize();

            int id = BitConverter.ToInt32(message, offset);

            Quaternion quaternion = new Quaternion(BitConverter.ToSingle(message, offset + sizeof(int)),
                                                   BitConverter.ToSingle(message, offset + sizeof(int) + sizeof(float)),
                                                   BitConverter.ToSingle(message, offset + sizeof(int) + sizeof(float) * 2),
                                                   BitConverter.ToSingle(message, offset + sizeof(int) + sizeof(float) * 3));

            (int turretId, Quaternion rotation) outData = new(id, quaternion);

            return outData;
        }

        public MessageHeader GetMessageHeader(float admissionTime)
        {
            return new MessageHeader((int)GetMessageType(), lastMessageId);
        }

        public static int GetHeaderSize()
        {
            return sizeof(bool) + sizeof(int) * MessageHeader.amountIntsInSendTime + sizeof(int) + sizeof(int);
        }

        public static GAME_MESSAGE_TYPE GetMessageType()
        {
            return GAME_MESSAGE_TYPE.TURRET_ROTATION;
        }

        public byte[] Serialize(float admissionTime = -1)
        {
            List<byte> outData = new List<byte>();

            MessageHeader messageHeader = GetMessageHeader(admissionTime);

            outData.AddRange(messageHeader.Bytes);

            outData.AddRange(BitConverter.GetBytes(data.turretId));
            outData.AddRange(BitConverter.GetBytes(data.rotation.x));
            outData.AddRange(BitConverter.GetBytes(data.rotation.y));
            outData.AddRange(BitConverter.GetBytes(data.rotation.z));
            outData.AddRange(BitConverter.GetBytes(data.rotation.w));

            return outData.ToArray();
        }

        public static int GetMessageSize()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
