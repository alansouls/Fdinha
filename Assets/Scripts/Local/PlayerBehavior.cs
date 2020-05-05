using Assets.Scripts.Entites;
using Assets.Scripts.Local;
using Assets.Scripts.Messages;
using Assets.Scripts.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerBehavior : MonoBehaviour
{
    public Player Player;
    public CardBehaviour Card1;
    public CardBehaviour Card2;
    public CardBehaviour Card3;
    public Button[] GuessButtons;
    public Stack<Card> Table { get; set; }
    public IDictionary<Player, int> Guesses { get; set; }
    public IDictionary<Player, int> Wins { get; set; }
    public bool CanPlay;
    public bool GuessingRound;
    public GameClient GameClient;
    public bool Host;
    public Button StartGameButton;
    public Text PlayerCountText;
    public bool IsReady;
    public Button ReadyButton;
    public Text HostIp;
    public CardBehaviour[] TopCardsTable;
    public string CurrentCanPlaySprite;
    public Image CanPlayImage;
    public Text[] PlayersInfo;
    public Text PlayerNameText;

    // Start is called before the first frame update
    void Start()
    {
        Table = new Stack<Card>();
        Guesses = new Dictionary<Player, int>();
        Wins = new Dictionary<Player, int>();
        StartGameButton.gameObject.SetActive(false);
        PlayerCountText.gameObject.SetActive(false);
        CanPlay = false;
        GuessingRound = true;
        if (Host)
        {
            GameClient = new GameClient(7777, 8965);
        }
        else
        {
            GameClient = new GameClient(7778, 8965);
        }
        GameClient.Player = this;
        Player = new Player
        {
            Id = Guid.NewGuid().ToString(),
            Cards = new List<Card>(),
            Lives = 3,
            Valid = true
        };
        StartGameButton.gameObject.SetActive(Host);
        PlayerCountText.gameObject.SetActive(Host);
    }

    // Update is called once per frame
    void Update()
    {
        if (CanPlay == true && Player.Cards.Count == 0)
        {
            CanPlay = false;
            Pass();
        }
        var card1 = Player.Cards.ElementAtOrDefault(0);
        var card2 = Player.Cards.ElementAtOrDefault(1);
        var card3 = Player.Cards.ElementAtOrDefault(2);
        var tableCard1 = Table.Any() ? Table.Pop() : new Card();
        var tableCard2 = Table.Any() ? Table.Pop() : new Card();
        var tableCard3 = Table.Any() ? Table.Pop() : new Card();
        Table.Push(tableCard3);
        Table.Push(tableCard2);
        Table.Push(tableCard1);
        BindTableCards(tableCard1, tableCard2, tableCard3);
        HandleTableCardBehaviourActiveness(tableCard1, tableCard2, tableCard3);
        HandleCardBehaviourActiveness(card1, card2, card3);
        BindCardsToBehaviour(card1, card2, card3);
        VerifyGuessButtonsActiveness();
        VerifyIfCanPlay();
        CheckIfCanPlay();
        ReadyButton.gameObject.SetActive(!IsReady);
        HandleCanPlaySprite();
        AdjustPlayersInfo();
    }

    public void OnDestroy()
    {
        GameClient.Close();
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("Menu");
    }
    
    public void AdjustPlayersInfo()
    {
        var players = Wins.Keys.ToList();
        int i = 0;
        foreach (var player in players)
        {
            var playerText = PlayersInfo[i];
            if (!playerText.gameObject.activeSelf)
                playerText.gameObject.SetActive(true);
            playerText.text = $"{player.Name} ({Guesses[player]}/{Wins[player]})";
            ++i;
        }
        for (int j = i; j < PlayersInfo.Count(); j++)
        {
            if (PlayersInfo[j].gameObject.activeSelf)
                PlayersInfo[j].gameObject.SetActive(false);
        }
    }

    private void HandleCanPlaySprite()
    {
        string canPlaySprite;
        if (!CanPlay)
        {
            canPlaySprite = "bolavermelhatransparente";
        }
        else
        {
            canPlaySprite = "bolaverdetransparente";
        }
        if (canPlaySprite != CurrentCanPlaySprite)
        {
            var sprite = Resources.Load<Sprite>($"Bolas/{canPlaySprite}");
            CanPlayImage.sprite = sprite;
            CurrentCanPlaySprite = canPlaySprite;
        }
    }

    private void BindTableCards(Card tableCard1, Card tableCard2, Card tableCard3)
    {
        if (tableCard1 != TopCardsTable[0].Card)
            TopCardsTable[0].Card = tableCard1;
        if (tableCard2 != TopCardsTable[1].Card)
            TopCardsTable[1].Card = tableCard2;
        if (tableCard3 != TopCardsTable[2].Card)
            TopCardsTable[2].Card = tableCard3;
    }

    public void CreateGame()
    {
        var ip = IPAddress.Parse("127.0.0.1");
        StartGameButton.gameObject.SetActive(true);
        PlayerCountText.gameObject.SetActive(true);
        JoinGame(ip);
    }

    public void JoinGame()
    {
        bool valid = IPAddress.TryParse(HostIp.text, out IPAddress ip);

        if (valid)
            JoinGame(ip);
        else
        {
            Debug.Log("Invalid ip");
        }
    }

    public void JoinGame(IPAddress serverIp)
    {
        Player.Name = PlayerNameText.text;
        GameClient.DefineServerEP(serverIp);
        GameClient.ListenServerUpdates();
        GameClient.SendCommandToServer(new MessageModel
        {
            MessageId = Guid.NewGuid().ToString(),
            Action = new ActionObject
            {
                Action = Assets.Scripts.Enums.Action.ADD_PLAYER,
                Player = Player
            }
        });
        IsReady = true;
    }

    public void Pass()
    {
        GameClient.SendCommandToServer(new MessageModel
        {
            MessageId = Guid.NewGuid().ToString(),
            Action = new ActionObject
            {
                Action = Assets.Scripts.Enums.Action.PASS,
                Player = Player
            }
        });
    }

    public void DefineHost()
    {
        Host = true;
    }

    private void CheckIfCanPlay()
    {
        if (CanPlay && !Player.Cards.Any())
        {
            CanPlay = false;
            GameClient.SendCommandToServer(new MessageModel
            {
                MessageId = Guid.NewGuid().ToString(),
                Action = new ActionObject
                {
                    Action = Assets.Scripts.Enums.Action.PASS,
                    Player = Player
                }
            });
        }
    }

    private void VerifyIfCanPlay()
    {
        bool canPlay = CanPlay && !GuessingRound;
        Card1.CanPlay = canPlay;
        Card2.CanPlay = canPlay;
        Card3.CanPlay = canPlay;
    }

    private void VerifyGuessButtonsActiveness()
    {
        foreach (var button in GuessButtons)
        {
            if (CanPlay && GuessingRound && !button.gameObject.activeSelf)
            {
                button.gameObject.SetActive(true);
            }
            else if ((!CanPlay || !GuessingRound) && button.gameObject.activeSelf)
            {
                button.gameObject.SetActive(false);
            }
        }
    }

    public void MakeGuess(int guess)
    {
        CanPlay = false;
        GameClient.SendCommandToServer(new MessageModel
        {
            MessageId = Guid.NewGuid().ToString(),
            Action = new ActionObject
            {
                Action = Assets.Scripts.Enums.Action.GUESS,
                Guess = guess,
                Player = Player
            }
        });
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
        CanPlay = false;
        GameClient.SendCommandToServer(new MessageModel
        {
            MessageId = Guid.NewGuid().ToString(),
            Action = new ActionObject
            {
                Action = Assets.Scripts.Enums.Action.PLAY_CARD,
                Player = Player,
                Card = cardToRemove,
            }
        });
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

    private void HandleTableCardBehaviourActiveness(Card card1, Card card2, Card card3)
    {
        if (card1 == null && TopCardsTable[0].gameObject.activeSelf == true)
        {
            TopCardsTable[0].gameObject.SetActive(false);
        }
        else if (card1 != null && TopCardsTable[0].gameObject.activeSelf == false)
        {
            TopCardsTable[0].gameObject.SetActive(true);
        }
        if (card2 == null && TopCardsTable[1].gameObject.activeSelf == true)
        {
            TopCardsTable[1].gameObject.SetActive(false);
        }
        else if (card2 != null && TopCardsTable[1].gameObject.activeSelf == false)
        {
            TopCardsTable[1].gameObject.SetActive(true);
        }
        if (card3 == null && TopCardsTable[2].gameObject.activeSelf == true)
        {
            TopCardsTable[2].gameObject.SetActive(false);
        }
        else if (card3 != null && TopCardsTable[2].gameObject.activeSelf == false)
        {
            TopCardsTable[2].gameObject.SetActive(true);
        }
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

    public void StartGame()
    {
        GameClient.SendCommandToServer(new MessageModel
        {
            MessageId = Guid.NewGuid().ToString(),
            Action = new ActionObject
            {
                Action = Assets.Scripts.Enums.Action.START_GAME,
            }
        });
    }
}
