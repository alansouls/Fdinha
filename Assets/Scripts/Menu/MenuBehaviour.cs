using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuBehaviour : MonoBehaviour
{
    public Button HostButton;
    public Button GuestButton;
    public Text HostIp;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void HostGame()
    {
        SceneManager.LoadScene("HostScene");
    }

    public void JoinGame()
    {
        SceneManager.LoadScene("GuestScene");
    }
}
