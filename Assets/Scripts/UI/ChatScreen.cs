using System;

using UnityEngine;
using UnityEngine.UI;

public class ChatScreen : MonoBehaviour
{
    #region EXPOSED_FIELDS
    [SerializeField] private Text messages = null;
    [SerializeField] private InputField inputMessage = null;
    #endregion

    #region ACTIONS
    public Action<string> onSendChat;
    #endregion

    #region UNITY_CALLS
    private void Start()
    {
        inputMessage.onEndEdit.AddListener(OnEndEdit);

        NetworkManager.Instance.onStartConnection += DisableInputIfServer;

        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (NetworkManager.Instance != null)
        { 
            NetworkManager.Instance.onStartConnection -= DisableInputIfServer; 
        }
    }
    #endregion

    #region PUBLIC_METHODS
    public void AddText(string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return;
        }

        messages.text += str + "\n";
    }
    #endregion

    #region PRIVATE_METHODS
    private void DisableInputIfServer(bool isServer)
    {
        inputMessage.gameObject.SetActive(!isServer);
    }

    private void OnEndEdit(string str)
    {
        if (str == "")
        {
            return;
        }

        onSendChat.Invoke(str);

        inputMessage.ActivateInputField();
        inputMessage.Select();
        inputMessage.text = "";
    }
    #endregion
}
