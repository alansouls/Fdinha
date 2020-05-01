using Assets.Scripts.Entites;
using Assets.Scripts.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Local
{
    [Serializable]
    public class ActionObject
    {
        public Player Player;
        public Enums.Action Action;
        public Card Card;
        public int Guess;
    }
}
