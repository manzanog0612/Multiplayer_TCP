using Game.Common.Player;

namespace Game.Common
{
    public class SessionHandler : MonoBehaviourSingleton<SessionHandler>
    {
        #region PRIVATE_FIELDS
        private PlayerModel playerModel = null;
        #endregion

        #region PUBLIC_METHODS
        public void SetPlayerName(string name)
        {
            playerModel.SetName(name);
        }
        #endregion

        #region OVERRIDE_METHODS 
        protected override void Initialize()
        {
            playerModel = new PlayerModel();
        }
        #endregion
    }
}
