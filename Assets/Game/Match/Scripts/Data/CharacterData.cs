using MultiplayerLibrary.Reflection.Attributes;

namespace Game.Match.Data
{
    public class CharacterData
    {
        #region PRIVATE_METHODS
        [SyncField] private int life = 100;
        #endregion

        #region PROPERTIES
        public int Life { get => life; }
        #endregion

        #region PUBLIC_METHODS
        public void SetLife(int life)
        {
            this.life = life;
        }

        public void LoseLife(int life)
        {
            this.life -= life;
        }
        #endregion
    }
}
