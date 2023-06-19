using System;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerLibrary.Reflection
{
    public static class NetDeserialization
    {
        #region PUBLIC_FIELDS
        public static int Deserialize(this int intValue, string name, byte[] data,  ref int offset, out bool success)
        {
            Debug.Log(intValue);

            int result = BitConverter.ToInt32(data, offset);

            offset += sizeof(int);

            success = MessageIsSane(name, data, sizeof(int), ref offset, result);

            return result;
        }

        public static float Deserialize(this float floatValue, string name, byte[] data, ref int offset, out bool success)
        {
            Debug.Log(floatValue);

            float result = BitConverter.ToSingle(data, offset);

            offset += sizeof(float);

            success = MessageIsSane(name, data, sizeof(float), ref offset, (int)(result * 100f));

            return result;
        }

        public static bool Deserialize(this bool boolValue, string name, byte[] data, ref int offset, out bool success)
        {
            Debug.Log(boolValue);

            bool result = BitConverter.ToBoolean(data, offset);

            offset += sizeof(bool);

            success = MessageIsSane(name, data, sizeof(bool), ref offset, result ? 1 : 0);

            return result;
        }

        public static char Deserialize(this char charValue, string name, byte[] data, ref int offset, out bool success)
        {
            Debug.Log(charValue);

            char result = BitConverter.ToChar(data, offset);

            offset += sizeof(char);

            success = MessageIsSane(name, data, sizeof(char), ref offset, result);

            return result;
        }

        public static string Deserialize(this string stringValue, string name, byte[] data, ref int offset, out bool success)
        {
            Debug.Log(stringValue);

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

            success = MessageIsSane(name, data, sizeof(char) * stringLenght, ref offset, operation);

            return result;
        }

        public static Vector2 Deserialize(this Vector2 vector2Value, string name, byte[] data, ref int offset, out bool success)
        {
            Debug.Log(vector2Value);

            Vector2 result = Vector2.zero;
            result.x = BitConverter.ToSingle(data, offset);
            result.y = BitConverter.ToSingle(data, offset + sizeof(float));

            offset += sizeof(float) * 2;

            int operation = (int)(result.x * 100) + (int)(result.y * 100);

            success = MessageIsSane(name, data, sizeof(float) * 2, ref offset, operation);

            return result;
        }

        public static Vector3 Deserialize(this Vector3 vector3Value, string name, byte[] data, ref int offset, out bool success)
        {
            Debug.Log(vector3Value);

            Vector3 result = Vector3.zero;
            result.x = BitConverter.ToSingle(data, offset);
            result.y = BitConverter.ToSingle(data, offset + sizeof(float));
            result.z = BitConverter.ToSingle(data, offset + sizeof(float) * 2);

            offset += sizeof(float) * 3;

            int operation = (int)(result.x * 100) + (int)(result.y * 100) + (int)(result.z * 100);

            success = MessageIsSane(name, data, sizeof(float) * 3, ref offset, operation);

            return result;
        }

        public static Quaternion Deserialize(this Quaternion quaternionValue, string name, byte[] data, ref int offset, out bool success)
        {
            Debug.Log(quaternionValue);

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

        public static Color Deserialize(this Color colorValue, string name, byte[] data, ref int offset, out bool success)
        {
            Debug.Log(colorValue);

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

        public static Transform Deserialize(this Transform transformValue, string name, byte[] data, ref int offset, out bool success)
        {
            Debug.Log(transformValue);

            Transform result = null;
            //result.r = BitConverter.ToSingle(data, offset);
            //result.g = BitConverter.ToSingle(data, offset + sizeof(float));
            //result.b = BitConverter.ToSingle(data, offset + sizeof(float) * 2);
            //result.a = BitConverter.ToSingle(data, offset + sizeof(float) * 3);
            //
            //offset += sizeof(float) * 4;
            //
            //int operation = (int)(result.r * 100) + (int)(result.g * 100) + (int)(result.b * 100) + (int)(result.a * 100);

            success = MessageIsSane(name, data, 0, ref offset, 0);

            return result;
        }

        public static byte[] Deserialize(this byte[] byteArrayValue, string name, byte[] data, ref int offset, out bool success)
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

            return messageSize == messageBodySize + headerSize + sizeof(int) * 2 && tailOperation == operation;
        }
        #endregion
    }
}
