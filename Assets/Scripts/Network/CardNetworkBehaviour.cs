using Assets.Scripts.Entites;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

#pragma warning disable CS0618 // O tipo ou membro é obsoleto
public class CardNetworkBehaviour : NetworkBehaviour
#pragma warning restore CS0618 // O tipo ou membro é obsoleto
{
    [SyncVar]
    public Card Card;
    public Button PlayCardButton;
    public Button CancellCardButton;
    public bool CanPlay;
    public int Index;
    // Start is called before the first frame update
    void Start()
    {
        var buttonGrouper = GameObject.Find($"CardButton{Index}");
        CancellCardButton = buttonGrouper.transform.GetChild(0).gameObject.GetComponent<Button>();
        PlayCardButton = buttonGrouper.transform.GetChild(1).gameObject.GetComponent<Button>();
        HideButtons();
        CanPlay = false;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnMouseUp()
    {
        if (CanPlay)
            ShowButtons();
    }

    public void HideButtons()
    {
        PlayCardButton.gameObject.SetActive(false);
        CancellCardButton.gameObject.SetActive(false);
    }

    public void ShowButtons()
    {
        PlayCardButton.gameObject.SetActive(true);
        CancellCardButton.gameObject.SetActive(true);
    }
}
