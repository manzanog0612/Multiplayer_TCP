using System;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerLibrary.Reflection
{
    public static class NetDeserialization
    {
        #region DESERIALIZE_METHODS
        public static int DeserializeInt(string name, byte[] data,  ref int offset, out bool success)
        {
            int result = BitConverter.ToInt32(data, offset);

            offset += sizeof(int);

            success = MessageIsSane(name, data, sizeof(int), ref offset, result);

            return result;
        }

        public static float DeserializeFloat(string name, byte[] data, ref int offset, out bool success)
        {
            float result = BitConverter.ToSingle(data, offset);

            offset += sizeof(float);

            success = MessageIsSane(name, data, sizeof(float), ref offset, (int)(result * 100f));

            return result;
        }

        public static bool DeserializeBool(string name, byte[] data, ref int offset, out bool success)
        {
            bool result = BitConverter.ToBoolean(data, offset);

            offset += sizeof(bool);

            success = MessageIsSane(name, data, sizeof(bool), ref offset, result ? 1 : 0);

            return result;
        }

        public static char DeserializeChar(string name, byte[] data, ref int offset, out bool success)
        {
            char result = BitConverter.ToChar(data, offset);

            offset += sizeof(char);

            success = MessageIsSane(name, data, sizeof(char), ref offset, result);

            return result;
        }

        public static string DeserializeString(string name, byte[] data, ref int offset, out bool success)
        {
            int stringLenght = BitConverter.ToInt32(data, offset);
            offset += sizeof(int);

            string result = string.Empty;
            int operation = 0;

            for (int i = 0; i < stringLenght; i++)
            {
                char charValue = BitConverter.ToChar(data, offset);

                result += charValue;
                offset += sizeof(char);
                operation += charValue;
            }

            success = MessageIsSane(name, data, sizeof(char) * stringLenght + sizeof(int), ref offset, operation);

            return result;
        }

        public static Vector2 DeserializeVector2(string name, byte[] data, ref int offset, out bool success)
        {
            Vector2 result = Vector2.zero;
            result.x = BitConverter.ToSingle(data, offset);
            result.y = BitConverter.ToSingle(data, offset + sizeof(float));

            offset += sizeof(float) * 2;

            int operation = (int)(result.x * 100) + (int)(result.y * 100);

            success = MessageIsSane(name, data, sizeof(float) * 2, ref offset, operation);

            return result;
        }

        public static Vector3 DeserializeVector3(string name, byte[] data, ref int offset, out bool success)
        {
            Vector3 result = Vector3.zero;
            result.x = BitConverter.ToSingle(data, offset);
            result.y = BitConverter.ToSingle(data, offset + sizeof(float));
            result.z = BitConverter.ToSingle(data, offset + sizeof(float) * 2);

            offset += sizeof(float) * 3;

            int operation = (int)(result.x * 100) + (int)(result.y * 100) + (int)(result.z * 100);

            success = MessageIsSane(name, data, sizeof(float) * 3, ref offset, operation);

            return result;
        }

        public static Quaternion DeserializeQuaternion(string name, byte[] data, ref int offset, out bool success)
        {
            Quaternion result = Quaternion.identity;
            result.x = BitConverter.ToSingle(data, offset);
            result.y = BitConverter.ToSingle(data, offset + sizeof(float));
            result.z = BitConverter.ToSingle(data, offset + sizeof(float) * 2);
            result.w = BitConverter.ToSingle(data, offset + sizeof(float) * 3);

            offset += sizeof(float) * 4;

            int operation = (int)(result.x * 100) + (int)(result.y * 100) + (int)(result.z * 100) + (int)(result.w * 100);

            success = MessageIsSane(name, data, sizeof(float) * 4, ref offset, operation);

            return result;
        }

        public static Color DeserializeColor(string name, byte[] data, ref int offset, out bool success)
        {
            Color result = Color.clear;
            result.r = BitConverter.ToSingle(data, offset);
            result.g = BitConverter.ToSingle(data, offset + sizeof(float));
            result.b = BitConverter.ToSingle(data, offset + sizeof(float) * 2);
            result.a = BitConverter.ToSingle(data, offset + sizeof(float) * 3);

            offset += sizeof(float) * 4;

            int operation = (int)(result.r * 100) + (int)(result.g * 100) + (int)(result.b * 100) + (int)(result.a * 100);

            success = MessageIsSane(name, data, sizeof(float) * 4, ref offset, operation);

            return result;
        }

        public static (Vector3, Quaternion, Vector3) DeserializeTransform(string name, byte[] data, ref int offset, out bool success)
        {
            Vector3 position = new Vector3(BitConverter.ToSingle(data, offset), 
                                           BitConverter.ToSingle(data, offset + sizeof(float)), 
                                           BitConverter.ToSingle(data, offset + sizeof(float) * 2));
            Quaternion rotation = new Quaternion(BitConverter.ToSingle(data, offset + sizeof(float) * 3), 
                                                 BitConverter.ToSingle(data, offset + sizeof(float) * 4), 
                                                 BitConverter.ToSingle(data, offset + sizeof(float) * 5), 
                                                 BitConverter.ToSingle(data, offset + sizeof(float) * 6));
            Vector3 localScale = new Vector3(BitConverter.ToSingle(data, offset + sizeof(float) * 7),
                                             BitConverter.ToSingle(data, offset + sizeof(float) * 8),
                                             BitConverter.ToSingle(data, offset + sizeof(float) * 9));

            int operation = (int)(position.x * 100) + (int)(position.y * 100) + (int)(position.z * 100) +
                            (int)(rotation.x * 100) + (int)(rotation.y * 100) + (int)(rotation.z * 100) + (int)(rotation.w * 100) +
                            (int)(localScale.x * 100) + (int)(localScale.y * 100) + (int)(localScale.z * 100);

            //vector3 - quaternion - vector3
            int messageSize = sizeof(float) * 3 + sizeof(float) * 4 + sizeof(float) * 3;
            offset += messageSize;

            success = MessageIsSane(name, data, messageSize, ref offset, operation);

            return (position, rotation, localScale);
        }

        public static byte[] DeserializeBytes(string name, byte[] data, ref int offset, out bool success)
        {
            int operation = 0;

            List<byte> result = new List<byte>();
            int arrayLenght = BitConverter.ToInt32(data, offset);

            offset += sizeof(int);

            for (int i = 0; i < arrayLenght; i++)
            {
                operation += data[i + offset];
                result.Add(data[i + offset]);
            }

            offset += arrayLenght;

            success = MessageIsSane(name, data, arrayLenght + sizeof(int), ref offset, operation);

            return result.ToArray();
        }
        #endregion

        #region PRIVATE_METHODS
        private static (int operation, int messageSize) GetTail(byte[] data, ref int offset)
        {
            int operation = BitConverter.ToInt32(data, offset);
            int messageSize = BitConverter.ToInt32(data, offset + sizeof(int));

            offset += sizeof(int) * 2;

            return (operation, messageSize);
        }

        private static bool MessageIsSane(string name, byte[] data, int messageBodySize, ref int offset, int operation)
        {
            (int tailOperation, int messageSize) = GetTail(data, ref offset);

            int headerSize = name.Length * sizeof(char) + sizeof(int);
            int messageReceivedSize = messageBodySize + headerSize + sizeof(int) * 2;
            return messageSize == messageReceivedSize && tailOperation == operation;
        }
        #endregion
    }
}
