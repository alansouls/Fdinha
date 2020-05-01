using Assets.Scripts.Entites;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Player
{
    public bool Valid;
    public int Lives;
    public string Name;
    public List<Card> Cards;
    public uint Id;

    public static bool operator ==(Player a, Player b)
    {
        return a.Id == b.Id;
    }

    public static bool operator !=(Player a, Player b)
    {
        return a.Id != b.Id;
    }
}
