using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public static class BoolSerialization
{
    public static List<byte> Serialize(object data)
    {
        bool value = (bool)data;
        Debug.Log(value);

        List<byte> bytes = new List<byte>();
        bytes.AddRange(BitConverter.GetBytes((int)VALUE_TYPE.BOOL).ToList());
        bytes.AddRange(BitConverter.GetBytes(value).ToList());

        return bytes;
    }
}
