using System;

using UnityEngine;
using UnityEngine.UI;

public class ChatScreen : MonoBehaviour
{
    #region EXPOSED_FIELDS
    [SerializeField] private Text messages = null;
    [SerializeField] private InputField inputMessage = null;
    [SerializeField] private int maxMessageIndex = 19;
    #endregion

    #region ACTIONS
    public Action<string> onSendChat;
    #endregion

    #region PRIVATE_FIELDS
    private int messsageIndex = 0;
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

        SetMessageIndex();

        messages.text += str + "\n";
    }
    #endregion

    #region PRIVATE_METHODS
    private void SetMessageIndex()
    {
        if (messsageIndex > maxMessageIndex)
        {
            messages.text = string.Empty;
            messsageIndex = 0;
        }

        messsageIndex++;
    }
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
