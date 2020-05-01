using Assets.Scripts.Entites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Local
{
    public class ResponseMessage
    {
        public Guid Id { get; set; }

        public Player Player { get; set; }

        public bool AdjustPlayer { get; set; }

        public bool CanPlay { get; set; }

        public bool GuessingRound { get; set; }

        public Stack<Card> Table { get; set; }

        public IDictionary<Player, int> Guesses { get; set; }

        public IDictionary<Player, int> Wins { get; set; }
    }
}
