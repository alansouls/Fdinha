using Assets.Scripts.Entites;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct Player
{
    public bool Valid;
    public int Lives;
    public string Name;
    public List<Card> Cards;
    public Guid Id;

    public override bool Equals(object obj)
    {
        return obj is Player player &&
               Id == player.Id;
    }

    public override int GetHashCode()
    {
        return 2108858624 + Id.GetHashCode();
    }

    public static bool operator ==(Player a, Player b)
    {
        return a.Id == b.Id;
    }

    public static bool operator !=(Player a, Player b)
    {
        return a.Id != b.Id;
    }
}
