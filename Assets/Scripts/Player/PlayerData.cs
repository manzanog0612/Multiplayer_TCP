using UnityEngine;

public class PlayerData
{
    public int id = -1;
    public Vector2? movement = null;
    public string message = null;

    public bool IdIsVoid()
    {
        return id == -1;
    }
}
