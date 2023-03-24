using System;

using UnityEngine;

public class PlayerHandler : MonoBehaviour
{
    #region EXPOSED_FIELDS
    [SerializeField] private MovableSquare square = null;
    [SerializeField] private int speed = 50;
    [SerializeField] private bool controlsSquare = false;
    #endregion

    #region PRIVATE_FIELDS
    private bool initialized = false;
    private PlayerData playerData = new PlayerData();

    private Vector2 movement = Vector2.zero;
    #endregion

    #region ACTIONS
    private Action<PlayerData> onChangePlayerData = null;
    private Action<string> onReceiveMessage = null;
    #endregion    

    #region UNITY_CALLS
    public void Update()
    {
        if (!initialized)
        {
            return;
        }

        DetectInput();

        Processinput();

        ResetData();
    }
    #endregion

    #region PUBLIC_METHODS
    public void Init(Action<PlayerData> onChangePlayerData, Action<string> onReceiveMessage)
    {
        this.onChangePlayerData = onChangePlayerData;
        this.onReceiveMessage += onReceiveMessage;

        initialized = true;
    }

    public void SetPlaterControlsSquare(bool controlsSquare)
    {
        this.controlsSquare = controlsSquare;
    }

    public void SetPlayerData(PlayerData playerData)
    {
        if (playerData.movement != null)
        { 
            square.Move((Vector2)playerData.movement);
        }  
        
        if (playerData.message != null)
        {
            onReceiveMessage.Invoke(playerData.message);
        }
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
        if (!controlsSquare)
        {
            return;
        }

        movement = Vector2.zero;

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
    }

    private void Processinput()
    {
        if (movement != Vector2.zero)
        {
            square.Move(movement);
            playerData.movement = movement;
            onChangePlayerData.Invoke(playerData);
        }
    }

    private void ResetData()
    {
        playerData.movement = null;
        playerData.message = null;
    }
    #endregion
}
