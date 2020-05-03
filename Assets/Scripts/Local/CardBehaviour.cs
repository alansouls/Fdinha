using Assets.Scripts.Entites;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.UI;

public class CardBehaviour : MonoBehaviour
{
    public Card Card;
    public Button PlayCardButton;
    public Button CancellCardButton;
    public bool CanPlay;
    public SpriteRenderer Renderer;
    public string SpriteName;
    public bool IsTable;
    // Start is called before the first frame update
    void Start()
    {
        HideButtons();
        CanPlay = false;
        SpriteName = "";
    }

    // Update is called once per frame
    void Update()
    {
        var spriteName = $"card_b_{GetCardSpriteName()}_large";
        if (spriteName != SpriteName)
        {
            var sprite = Resources.Load<Sprite>($"Cards/{spriteName}");
            Renderer.sprite = sprite;
            SpriteName = spriteName;
        }
    }

    public string GetCardSpriteName()
    {
        var name = "";
        name += GetSuitLetter();
        name += GetValueLetter();

        return name;
    }

    public string GetValueLetter()
    {
        var value = Card.Value;
        if (value == 0)
            return "a";
        else if (value > 0 && value < 10)
        {
            return (value + 1).ToString();
        }
        else if(value == 10) 
        {
            return "j";
        }
        else if (value == 11)
        {
            return "q";
        }
        else if (value == 12)
        {
            return "k";
        }

        return "a";
    }

    public string GetSuitLetter()
    {
        switch (Card.Suit)
        {
            case Assets.Scripts.Enums.Suit.CLUBS:
                return "c";
            case Assets.Scripts.Enums.Suit.DIAMONDS:
                return "d";
            case Assets.Scripts.Enums.Suit.HEARTS:
                return "h";
            case Assets.Scripts.Enums.Suit.SPADES:
                return "s";
            default:
                return "c";
        }
    }

    private void OnMouseUp()
    {
        if (CanPlay && !IsTable)
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
