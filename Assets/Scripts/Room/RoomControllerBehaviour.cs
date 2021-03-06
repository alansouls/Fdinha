using Assets.Scripts.Util;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;
using UnityEngine.UI;

public class RoomControllerBehaviour : MonoBehaviour
{
    const float x = -1.525879e-05f;
    const float y = 117f;
    const float w = 476.5123f;
    const float h = 49.25342f;
    const float distance = 5f;
    public RoomBehaviour[] Rooms;
    public GameClient GameClient;
    public RoomBehaviour RoomPrefab;
    public GameObject ScrollViewContent;
    // Start is called before the first frame update
    void Start()
    {
        var random = new System.Random();
        GameClient = new GameClient(random.Next(7779, 7999), 8965)
        {
            ServerEP = new IPEndPoint(GameStatus.ServerIP, 8965)
        };
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDestroy()
    {
        GameClient.Close();
    }

    public void RefreshRooms()
    {
        ClearRooms();
        var rooms = GameClient.GetServerRooms();
        Rooms = new RoomBehaviour[rooms.Count];
        var currentY = y;
        for (int i = 0; i < rooms.Count; i++)
        {
            if (!rooms[i].Open)
                continue;
            var room = Instantiate(RoomPrefab, ScrollViewContent.transform);
            var rect = room.gameObject.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(x, currentY);
            currentY -= (h + distance);
            room.Room = rooms[i];
        }
    }

    private void ClearRooms()
    {
        foreach (var room in Rooms)
            Destroy(room.gameObject);
    }
}
