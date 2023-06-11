using System;

using UnityEngine;

public static class NetDeserialization 
{
    #region PUBLIC_FIELDS
    public static int Deserialize(this int intValue, byte[] data, ref int offset)
    {
        Debug.Log(intValue);

        int result = BitConverter.ToInt32(data, offset);

        offset += sizeof(int);

        return result;
    }

    public static float Deserialize(this float floatValue, byte[] data, ref int offset)
    {
        Debug.Log(floatValue);

        float result = BitConverter.ToSingle(data, offset);

        offset += sizeof(float);

        return result;
    }

    public static bool Deserialize(this bool boolValue, byte[] data, ref int offset)
    {
        Debug.Log(boolValue);

        bool result = BitConverter.ToBoolean(data, offset);

        offset += sizeof(bool);

        return result;
    }

    public static char Deserialize(this char charValue, byte[] data, ref int offset)
    {
        Debug.Log(charValue);

        char result = BitConverter.ToChar(data, offset);

        offset += sizeof(char);

        return result;
    }

    public static string Deserialize(this string stringValue, byte[] data, ref int offset)
    {
        Debug.Log(stringValue);

        int stringLenght = BitConverter.ToInt32(data, offset);
        offset += sizeof(int);

        string result = string.Empty;

        for (int i = 0; i < stringLenght; i++)
        {
            result += BitConverter.ToChar(data, offset);
            offset += sizeof(char);
        }

        return result;
    }

    public static Vector3 Deserialize(this Vector3 vector3Value, byte[] data, ref int offset)
    {
        Debug.Log(vector3Value);

        Vector3 result = Vector3.zero;
        result.x = BitConverter.ToSingle(data, offset);
        result.y = BitConverter.ToSingle(data, offset + sizeof(float));
        result.z = BitConverter.ToSingle(data, offset + sizeof(float) * 2);

        offset += sizeof(float) * 3;

        return result;
    }
    #endregion
}
