using UnityEngine;

public class PlayerData
{
    public int id = -1;
    public Vector3? movement = null;
    [SyncField] public Vector3 position = Vector3.zero;
    public string message = null;

    public bool IdIsVoid()
    {
        return id == -1;
    }
}
