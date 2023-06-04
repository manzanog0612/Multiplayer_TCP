using System;
using UnityEngine;

public class PlayerHandler : MonoBehaviour
{
    #region EXPOSED_FIELDS
    [SerializeField] private int speed = 50;
    #endregion

    #region PRIVATE_FIELDS
    private bool initialized = false;
    [SyncField] private PlayerData playerData = new PlayerData();

    private Vector3 movement = Vector3.zero;
    #endregion

    #region ACTIONS
    private Action<PlayerData> onChangePlayerData = null;
    #endregion

    #region UNITY_CALLS
    public void Update()
    {
        if (!initialized || !Application.isFocused)
        {
            return;
        }

        DetectInput();

        Processinput();

        ResetData();
    }
    #endregion

    #region PUBLIC_METHODS
    public void Init(Action<PlayerData> onChangePlayerData)
    {
        this.onChangePlayerData = onChangePlayerData;

        initialized = true;
    }

    public void SendChat(string message)
    {
        playerData.message = message;
        onChangePlayerData.Invoke(playerData);
    }
    #endregion

    #region PRIVATE_METHODS
    private void DetectInput()
    {
        movement = Vector3.zero;

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            movement.x = -speed * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            movement.x = speed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.UpArrow))
        {
            movement.y = speed * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            movement.y = -speed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.Q))
        {
            movement.z = speed * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.E))
        {
            movement.z = -speed * Time.deltaTime;
        }
    }

    private void Processinput()
    {
        if (movement != Vector3.zero)
        {
            playerData.movement = movement;
            playerData.position += movement;
            onChangePlayerData.Invoke(playerData);
        }
    }

    private void ResetData()
    {
        movement = Vector3.zero;
        playerData.movement = null;
        playerData.message = null;
    }
    #endregion
}
