using Assets.Src.Components.Gameplay;
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

        readonly Dictionary<string, Player> players;

        public PlayerManager()
        {
            instance = this;

            players = new();
        }

        public static Player? GetPlayer(string id)
        {
            return instance?.players.TryGetValue(id, out Player player) ?? false ? player : null;
        }

        public static void AddPlayer(string id, PlayerController controller)
        {
            Player player = SaveManager.LoadPlayerData(id);
            instance?.players.Add(id, player);
            controller.Player.Value = player;
        }

    }
}
