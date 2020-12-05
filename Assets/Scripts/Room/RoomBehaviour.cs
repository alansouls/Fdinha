using Assets.Scripts.Util;
using FdinhaServer.Entities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RoomBehaviour : MonoBehaviour
{
    public ServerRoom Room;
    public Text PlayerNameText;
    // Start is called before the first frame update
    void Start()
    {
        PlayerNameText = GameObject.FindGameObjectWithTag("playerInputText").GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        var scene = SceneManager.GetActiveScene().name;
        if (scene != "RoomScene" && !gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        }
    }

    public void JoinGame()
    {
        var name = PlayerNameText.text;
        if (!name.Any())
            return;
        GameStatus.PlayerName = name;
        GameStatus.IsDedicated = true;
        GameStatus.IsHost = name == "Alan" ? true : false;
        GameStatus.ServerRoom = Room;
        SceneManager.LoadScene("GuestScene");
    }
}
