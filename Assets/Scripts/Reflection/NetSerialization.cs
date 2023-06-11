using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public static class NetSerialization
{
    #region PUBLIC_FIELDS
    public static List<byte> Serialize(this int intValue, string fieldName)
    {
        Debug.Log(intValue);

        List<byte> bytes = SerializeMessageName(fieldName);
        bytes.AddRange(BitConverter.GetBytes(intValue).ToList());

        return bytes;
    }

    public static List<byte> Serialize(this float floatValue, string fieldName)
    {
        Debug.Log(floatValue);

        List<byte> bytes = SerializeMessageName(fieldName);
        bytes.AddRange(BitConverter.GetBytes(floatValue).ToList());

        return bytes;
    }

    public static List<byte> Serialize(this bool boolValue, string fieldName)
    {
        Debug.Log(boolValue);

        List<byte> bytes = SerializeMessageName(fieldName);
        bytes.AddRange(BitConverter.GetBytes(boolValue).ToList());

        return bytes;
    }

    public static List<byte> Serialize(this char charValue, string fieldName)
    {
        Debug.Log(charValue);

        List<byte> bytes = SerializeMessageName(fieldName);
        bytes.AddRange(BitConverter.GetBytes(charValue).ToList());

        return bytes;
    }

    public static List<byte> Serialize(this string stringValue, string fieldName)
    {
        Debug.Log(stringValue);

        List<byte> bytes = SerializeMessageName(fieldName);

        bytes.AddRange(BitConverter.GetBytes(stringValue.Length));

        for (int i = 0; i < stringValue.Length; i++)
        {
            bytes.AddRange(BitConverter.GetBytes(stringValue[i]));
        }

        return bytes;
    }

    public static List<byte> Serialize(this Vector3 vector3Value, string fieldName)
    {
        Debug.Log(vector3Value);

        List<byte> bytes = SerializeMessageName(fieldName);
        bytes.AddRange(BitConverter.GetBytes(vector3Value.x).ToList());
        bytes.AddRange(BitConverter.GetBytes(vector3Value.y).ToList());
        bytes.AddRange(BitConverter.GetBytes(vector3Value.z).ToList());

        return bytes;
    }
    #endregion

    #region PRIVATE_FIELDS
    private static List<byte> SerializeMessageName(string fieldName)
    {
        List<byte> bytes = new List<byte>();

        int fieldnameSize = fieldName.Length;

        bytes.AddRange(BitConverter.GetBytes(fieldnameSize));

        for (int i = 0; i < fieldnameSize; i++)
        {
            bytes.AddRange(BitConverter.GetBytes(fieldName[i]));
        }

        return bytes;
    }
    #endregion
}
