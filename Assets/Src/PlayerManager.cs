using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable
namespace Assets.Src
{
    internal class PlayerManager
    {

        static PlayerManager? instance;

        readonly Dictionary<Guid, Player> players;

        public PlayerManager()
        {
            instance = this;

            players = new();
        }

        public static Player? GetPlayer(Guid id)
        {
            return instance?.players.TryGetValue(id, out Player player) ?? false ? player : null;
        }

        public static void AddPlayer(Guid id)
        {

        }

    }
}
