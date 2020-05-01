using Assets.Scripts.Entites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class PlayerBehavior : MonoBehaviour
{
    public Player Player;
    public CardBehaviour Card1;
    public CardBehaviour Card2;
    public CardBehaviour Card3;
    public MatchController MatchController;
    public Button[] GuessButtons;

    // Start is called before the first frame update
    void Start()
    {
        Player = new Player
        {
            Cards = new List<Card>(),
            Lives = 3
        };
        foreach (var button in GuessButtons)
        {
            button.onClick.AddListener(() => { MakeGuess(GuessButtons.ToList().IndexOf(button)); });
        }
    }

    // Update is called once per frame
    void Update()
    {
        var card1 = Player.Cards.ElementAtOrDefault(0);
        var card2 = Player.Cards.ElementAtOrDefault(1);
        var card3 = Player.Cards.ElementAtOrDefault(2);
        HandleCardBehaviourActiveness(card1, card2, card3);
        BindCardsToBehaviour(card1, card2, card3);
        VerifyGuessButtonsActiveness();
        VerifyIfCanPlay();
        CheckIfCanPlay();
    }

    private void CheckIfCanPlay()
    {
        if (MatchController.CurrentPlayer == Player && !Player.Cards.Any())
            MatchController.Pass(Player);
    }

    private void VerifyIfCanPlay()
    {
        bool canPlay = MatchController.CurrentPlayer == Player && !MatchController.IsGuessing;
        Card1.CanPlay = canPlay;
        Card2.CanPlay = canPlay;
        Card3.CanPlay = canPlay;
    }

    private void VerifyGuessButtonsActiveness()
    {
        foreach (var button in GuessButtons)
        {
            if (MatchController.CurrentPlayer == Player && MatchController.IsGuessing == true && !button.gameObject.activeSelf)
            {
                button.gameObject.SetActive(true);
            }
            else if ((MatchController.CurrentPlayer != Player || !MatchController.IsGuessing) && button.gameObject.activeSelf)
            {
                button.gameObject.SetActive(false);
            }
        }
    }

    public void MakeGuess(int guess)
    {
        if (!MatchController.Guess(Player, guess))
            Debug.Log("Can't guess this quantity, because of that rule");
    }

    public void PlayCard1()
    {
        var cardToRemove = Card1.Card;
        PlayCard(cardToRemove);
        Card1.HideButtons();
        Card2.HideButtons();
        Card3.HideButtons();
    }

    public void PlayCard2()
    {
        var cardToRemove = Card2.Card;
        PlayCard(cardToRemove);
        Card1.HideButtons();
        Card2.HideButtons();
        Card3.HideButtons();
    }

    public void PlayCard3()
    {
        var cardToRemove = Card3.Card;
        PlayCard(cardToRemove);
        Card1.HideButtons();
        Card2.HideButtons();
        Card3.HideButtons();
    }

    public void CancelCard()
    {
        Card1.HideButtons();
        Card2.HideButtons();
        Card3.HideButtons();
    }

    private void PlayCard(Card cardToRemove)
    {
        Player.Cards.Remove(cardToRemove);
        MatchController.PlayCard(Player, cardToRemove);
    }

    private void BindCardsToBehaviour(Card card1, Card card2, Card card3)
    {
        if (card1 != Card1.Card)
            Card1.Card = card1;
        if (card2 != Card2.Card)
            Card2.Card = card2;
        if (card3 != Card3.Card)
            Card3.Card = card3;
    }

    private void HandleCardBehaviourActiveness(Card card1, Card card2, Card card3)
    {
        if (card1 == null && Card1.gameObject.activeSelf == true)
        {
            Card1.gameObject.SetActive(false);
        }
        else if (card1 != null && Card1.gameObject.activeSelf == false)
        {
            Card1.gameObject.SetActive(true);
        }
        if (card2 == null && Card2.gameObject.activeSelf == true)
        {
            Card2.gameObject.SetActive(false);
        }
        else if (card2 != null && Card2.gameObject.activeSelf == false)
        {
            Card2.gameObject.SetActive(true);
        }
        if (card3 == null && Card3.gameObject.activeSelf == true)
        {
            Card3.gameObject.SetActive(false);
        }
        else if (card3 != null && Card3.gameObject.activeSelf == false)
        {
            Card3.gameObject.SetActive(true);
        }
    }
}
