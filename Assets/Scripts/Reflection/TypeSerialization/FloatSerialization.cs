using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public static class FloatSerialization
{
    public static List<byte> Serialize(object data)
    {
        float value = (float)data;
        Debug.Log(value);

        List<byte> bytes = new List<byte>();
        bytes.AddRange(BitConverter.GetBytes((int)VALUE_TYPE.FLOAT).ToList());
        bytes.AddRange(BitConverter.GetBytes(value).ToList());

        return bytes;
    }
}
