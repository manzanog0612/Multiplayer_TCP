namespace Game.RoomSelection.RoomsView
{
    public class RoomData
    {
        #region PRIVATE_FIELDS
        private int id = -1;
        private int playersIn = 0;
        private int playersMax = 0;
        private bool inMatch = false;
        private int matchTime = 0;
        #endregion

        #region PROPERTIES
        public int Id { get => id; }
        public int PlayersIn { get => playersIn; }
        public int PlayersMax { get => playersMax; }
        public bool InMatch { get => inMatch; }
        public int MatchTime { get => matchTime; }
        #endregion

        #region CONSTRUCTOR
        public RoomData(int id, int playersIn, int playersMax, int matchTime, bool inMatch)
        {
            this.id = id;
            this.playersIn = playersIn;
            this.playersMax = playersMax;
            this.inMatch = inMatch;
            this.matchTime = matchTime;
        }
        #endregion
    }
}
