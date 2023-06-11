namespace Game.RoomSelection.RoomsView
{
    public class RoomViewModel
    {
        #region PRIVATE_FIELDS
        private string roomName = string.Empty;
        private int playersIn = 0;
        private int playersMax = 0;
        private bool inMatch = false;
        #endregion

        #region PROPERTIES
        public string RoomName { get => roomName; }
        public int PlayersIn { get => playersIn; }
        public int PlayersMax { get => playersMax; }
        public bool InMatch { get => inMatch; }
        #endregion

        #region CONSTRUCTOR
        public RoomViewModel(string roomName, int playersIn, int playersMax, bool inMatch)
        {
            this.roomName = roomName;
            this.playersIn = playersIn;
            this.playersMax = playersMax;
            this.inMatch = inMatch;
        }
        #endregion
    }
}
