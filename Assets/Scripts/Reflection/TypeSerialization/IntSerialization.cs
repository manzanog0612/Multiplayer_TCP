using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public static class IntSerialization
{
    public static List<byte> Serialize(object data)
    {
        int value = (int)data;
        Debug.Log(value);

        List<byte> bytes = new List<byte>();
        bytes.AddRange(BitConverter.GetBytes((int)VALUE_TYPE.INT).ToList());
        bytes.AddRange(BitConverter.GetBytes(value).ToList());

        return bytes;
    }
}
