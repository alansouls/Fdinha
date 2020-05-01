using Assets.Scripts.Entites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

#pragma warning disable CS0618 // O tipo ou membro é obsoleto
public class PlayerNetworkBehavior : NetworkBehaviour
#pragma warning restore CS0618 // O tipo ou membro é obsoleto
{
    public Player Player;
    public CardBehaviour Card1;
    public CardBehaviour Card2;
    public CardBehaviour Card3;
    public Button[] GuessButtons;
    public Button StartGameButton;
    public GameObject MyCanvas;
    public Text PlayerCountText;
    [SyncVar]
    public bool CanPlay;
    [SyncVar]
    public bool GuessingRound;
    [SyncVar]
    public int TestSyncVar;
    public Text TestSyncVarText;
    public GameObject ActionPrefab;
    public MatchNetworkController MatchController;

    // Start is called before the first frame update
    void Start()
    {
        CanPlay = false;
        GuessingRound = true;
        MyCanvas.SetActive(isLocalPlayer);
        MyCanvas.GetComponent<Canvas>().enabled = isLocalPlayer;
        StartGameButton.gameObject.SetActive(isServer);
        PlayerCountText.gameObject.SetActive(isServer);
        if (isServer)
            MatchController = GameObject.FindGameObjectWithTag("Controller")?.GetComponent<MatchNetworkController>();
        Player = new Player
        {
            Cards = new List<Card>(),
            Lives = 3,
            Id = netId.Value,
            Valid = true,
            Name = "name"
        };
        GuessButtons = new Button[4];
        for (int i = 0; i <= 3; i++)
        {
            GuessButtons[i] = GameObject.Find($"Guess {i}").GetComponent<Button>();
        }
        foreach (var button in GuessButtons)
        {
            button.onClick.AddListener(() => { MakeGuess(GuessButtons.ToList().IndexOf(button)); });
        }
    }

    // Update is called once per frame
    void Update()
    {
        TestSyncVarText.text = TestSyncVar.ToString();
        var card1 = Player.Cards.ElementAtOrDefault(0);
        var card2 = Player.Cards.ElementAtOrDefault(1);
        var card3 = Player.Cards.ElementAtOrDefault(2);
        if (isLocalPlayer)
        {
            HandleCardBehaviourActiveness(card1, card2, card3);
            BindCardsToBehaviour(card1, card2, card3);
            VerifyGuessButtonsActiveness();
            VerifyIfCanPlay();
        }
    }

    private void VerifyIfCanPlay()
    {
        Card1.CanPlay = CanPlay && !GuessingRound;
        Card2.CanPlay = CanPlay && !GuessingRound;
        Card3.CanPlay = CanPlay && !GuessingRound;
    }

    private void VerifyGuessButtonsActiveness()
    {
        foreach (var button in GuessButtons)
        {
            if (CanPlay && GuessingRound)
            {
                button.gameObject.SetActive(true);
            }
            else if ((!CanPlay || !GuessingRound) && button.gameObject.activeSelf)
            {
                button.gameObject.SetActive(false);
            }
        }
    }

    [Command]
    public void CmdMakeGuess(int guess)
    {
        CanPlay = false;
        var action = Instantiate(ActionPrefab, transform);
        var actionComponent = action.GetComponent<ActionNetworkBehaviour>();
        actionComponent.Player = Player;
        actionComponent.Action = Assets.Scripts.Enums.Action.GUESS;
        actionComponent.Guess = guess;

        NetworkServer.Spawn(action);
    }

    public void MakeGuess(int guess)
    {
        CmdMakeGuess(guess);
    }

    public void PlayCard1()
    {
        var cardToRemove = Card1.Card;
        CmdPlayCard(cardToRemove);
        Card1.HideButtons();
        Card2.HideButtons();
        Card3.HideButtons();
    }

    public void PlayCard2()
    {
        var cardToRemove = Card2.Card;
        CmdPlayCard(cardToRemove);
        Card1.HideButtons();
        Card2.HideButtons();
        Card3.HideButtons();
    }

    public void PlayCard3()
    {
        var cardToRemove = Card3.Card;
        CmdPlayCard(cardToRemove);
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

    [Command]
    private void CmdPlayCard(Card cardToRemove)
    {
        Player.Cards.Remove(cardToRemove);
        CanPlay = false;
        var action = Instantiate(ActionPrefab, transform);
        var actionComponent = action.GetComponent<ActionNetworkBehaviour>();
        actionComponent.Player = Player;
        actionComponent.Action = Assets.Scripts.Enums.Action.PLAY_CARD;
        actionComponent.CardSuit = cardToRemove.Suit;
        actionComponent.CardValue = cardToRemove.Value;
        NetworkServer.Spawn(action);
    }

    public void StartGame()
    {
        if (!isServer && MatchController == null)
            return;
        MatchController.StartGame();
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

    [ClientRpc]
    public void RpcAddCard(Card card)
    {
        Player.Cards.Add(card);
    }

    [ClientRpc]
    public void RpcRemoveLife()
    {
        Player.Lives--;
    }
}
