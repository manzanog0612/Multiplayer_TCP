using System.Collections.Generic;

namespace Game.Match.Entity.Turret.Data
{
    public class TurretModel
    {
        public List<int> closePlayersIds = new List<int>();
    }

    public class TurretResult
    {
        public int playerToShootId = -1;
    }
}
