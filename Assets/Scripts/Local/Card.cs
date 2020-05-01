using Assets.Scripts.Enums;
using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine;

namespace Assets.Scripts.Entites
{
    public struct Card
    {
        public bool Valid;
        public int Value;
        public Suit Suit;

        public static bool operator <(Card a, Card b)
        {
            if (a.Value == b.Value)
                return a.Suit < b.Suit;
            else
                return a.Value < b.Value;
        }

        public static bool operator >(Card a, Card b)
        {
            if (a.Value == b.Value)
                return a.Suit > b.Suit;
            else
                return a.Value > b.Value;
        }

        public static bool operator !=(Card a, Card b)
        {
            return a.Value != b.Value || a.Suit != b.Suit;
        }

        public static bool operator ==(Card a, Card b)
        {
            return a.Value == b.Value && a.Suit == b.Suit;
        }

        public static bool operator ==(Card a, Card? b)
        {
            if (b == null)
                return !a.Valid;
            return a == b.Value;
        }

        public static bool operator !=(Card a, Card? b)
        {
            if (b == null)
                return a.Valid;
            return a == b.Value;
        }
    }
}
