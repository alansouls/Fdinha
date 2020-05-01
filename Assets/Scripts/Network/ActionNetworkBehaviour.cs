using Assets.Scripts.Enums;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ActionNetworkBehaviour : NetworkBehaviour
{
    [SyncVar]
    public Player Player;
    [SyncVar]
    public Action Action;
    [SyncVar]
    public int CardValue;
    [SyncVar]
    public Suit CardSuit;
    [SyncVar]
    public int Guess;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
