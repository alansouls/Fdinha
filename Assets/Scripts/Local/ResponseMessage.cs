using Assets.Scripts.Entites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Local
{
    [Serializable]
    public class ResponseMessage
    {
        public string Id;

        public Player Player;

        public bool AdjustPlayer;

        public bool CanPlay;

        public bool GuessingRound;

        public Stack<Card> Table;

        public IDictionary<Player, int> Guesses;

        public IDictionary<Player, int> Wins;
    }
}
