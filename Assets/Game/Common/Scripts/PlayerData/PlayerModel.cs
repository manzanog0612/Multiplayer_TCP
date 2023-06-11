namespace Game.Common.Player
{
    public class PlayerModel
    {
        #region PRIVATE_FIELDS
        private string name = string.Empty;
        #endregion

        #region PROPERTIES
        public string Name { get => name;}
        #endregion

        #region PUBLIC_METHODS
        public void SetName(string name)
        {
            this.name = name;
        }
        #endregion
    }
}