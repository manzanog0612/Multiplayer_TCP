using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public static class Vector3Serialization
{
    public static List<byte> Serialize(object data)
    {
        Vector3 value = (Vector3)data;
        Debug.Log(value);

        List<byte> bytes = new List<byte>();
        bytes.AddRange(BitConverter.GetBytes((int)VALUE_TYPE.VECTOR3).ToList());
        bytes.AddRange(BitConverter.GetBytes(value.x).ToList());
        bytes.AddRange(BitConverter.GetBytes(value.y).ToList());
        bytes.AddRange(BitConverter.GetBytes(value.z).ToList());

        return bytes;
    }
}
