using Assets.Scripts.Entites;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardBehaviour : MonoBehaviour
{
    public Card Card;
    public Button PlayCardButton;
    public Button CancellCardButton;
    public bool CanPlay;
    // Start is called before the first frame update
    void Start()
    {
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
