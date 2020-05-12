using Assets.Scripts.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomControllerBehaviour : MonoBehaviour
{
    const double x = -1.525879e-05;
    const double y = 117;
    const double w = 476.5123;
    const double h = 49.25342;
    public RoomBehaviour[] Rooms;
    public GameClient GameClient;
    // Start is called before the first frame update
    void Start()
    {
        var random = new System.Random();
        GameClient = new GameClient(random.Next(7777, 7999), 8965);
        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void RefreshRooms()
    {
        GameClient.ListenServerUpdates
    }

}
