using FdinhaServer.Entities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomBehaviour : MonoBehaviour
{
    public ServerRoom Room;
    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(gameObject);
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
}
