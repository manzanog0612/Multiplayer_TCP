using Game.Common.Networking;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Common
{
    public enum SCENES { LOGIN, ROOM_SELECTION, MATCH_CONFIGURATION, WAIT_ROOM, MATCH}

    public class SceneController : MonoBehaviour
    {
        #region PROTECTED_FIELDS
        protected SessionHandler sessionHandler = null;
        protected ClientHandler clientHandler = null;
        #endregion

        #region UNITY_CALLS
        private void Start()
        {
            Init();
        }
        #endregion

        #region PROTECTED_METHODS
        protected virtual void Init()
        {
            sessionHandler = FindObjectOfType<SessionHandler>();
            clientHandler = FindObjectOfType<ClientHandler>();
        }

        protected void ChangeScene(SCENES scene)
        {
            SceneManager.LoadScene((int)scene);
        }
        #endregion
    }
}