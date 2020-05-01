using Assets.Scripts.Entites;
using Assets.Scripts.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering;
using UnityEngine.UI;

#pragma warning disable CS0618 // O tipo ou membro é obsoleto
public class MatchNetworkController : NetworkBehaviour
#pragma warning restore CS0618 // O tipo ou membro é obsoleto
{
    public Stack<Card> Cards;
    public List<uint> Players;
    public int MaxRound;
    public Stack<Card> Table;
    public uint WinningPlayer;
    public uint CurrentPlayer;
    public bool IsGuessing;
    public Player LastPlayer;
    public IDictionary<uint, int> Guesses;
    public IDictionary<uint, int> Wins;
    public bool GameStarted;
    public int PlayerCount;

    // Start is called before the first frame update
    void Start()
    {
        Table = new Stack<Card>();
        Cards = new Stack<Card>();
        Players = new List<uint>();
        Wins = new Dictionary<uint, int>();
        Guesses = new Dictionary<uint, int>();
        MaxRound = 3;
        IsGuessing = false;
        GameStarted = false;
        PlayerCount = 0;
        GenerateCards();
        GetAllPlayers();
    }

    // Update is called once per frame
    void Update()
    {
        GetAllPlayers();
        GetNextAction();
    }

    private void GetNextAction()
    {
        var actions = GameObject.FindGameObjectsWithTag("Action");
        if (actions.Length > 0)
        {
            var action = actions[0].GetComponent<ActionNetworkBehaviour>();
            if (action.Player.Id == CurrentPlayer)
            {
                switch (action.Action)
                {
                    case Assets.Scripts.Enums.Action.PASS:
                        Pass(action.Player);
                        break;
                    case Assets.Scripts.Enums.Action.GUESS:
                        if (!Guess(action.Player, action.Guess))
                            ChangeCurrentPlayer(action.Player);
                        break;
                    case Assets.Scripts.Enums.Action.PLAY_CARD:
                        PlayCard(action.Player, new Card
                        {
                            Suit = action.CardSuit,
                            Value = action.CardValue
                        });
                        break;
                }
            }
            Destroy(action.gameObject);
        }
    }

    public void PlayCard(Player player, Card card)
    {
        if (player == LastPlayer)
        {
            PlaceCardInTable(player, card);
            AddWinToWinningPlayer();
            ChangeCurrentPlayer(NextPlayer(player));
        }
        else
        {
            PlaceCardInTable(player, card);
            ChangeCurrentPlayer(NextPlayer(player));
        }
        if (!Players.Where(p => GetPlayerById(p).Cards.Count > 0).Any())
        {
            RemoveLives();
            StartRound();
        }
        if (Players.Where(p => GetPlayerById(p).Lives > 0).Count() <= 1)
            EndGame();
    }

    private static Player GetPlayerById(uint p)
    {
        if (p <= 0)
            return new Player();
        return GetPlayerComponentById(p).Player;
    }

    public void Pass(Player player)
    {
        ChangeCurrentPlayer(NextPlayer(player));
    }

    private void EndGame()
    {
        if (Players.Any())
        {
            Debug.Log($"We have a winner!!! {GetPlayerById(Players.First()).Name} won the game");
        }else
        {
            Debug.Log($"The game ended in draw!!! That's rare...");
        }
    }

    private void PlaceCardInTable(Player player, Card card)
    {
        if (Table.Count == 0)
        {
            Table.Push(card);
            WinningPlayer = player.Id;
        }
        else
        {
            var topCard = Table.Peek();
            if (topCard < card)
            {
                Table.Push(card);
                WinningPlayer = player.Id;
            }
            else
            {
                topCard = Table.Pop();
                Table.Push(card);
                Table.Push(topCard);
            }
        }
    }

    private void DefinePlayersForNextRound()
    {
        ChangeCurrentPlayer(GetPlayerById(WinningPlayer));
        LastPlayer = PreviousPlayer(GetPlayerById(WinningPlayer));
    }

    private void AddWinToWinningPlayer()
    {
        if (Wins.ContainsKey(WinningPlayer))
            Wins[WinningPlayer] += 1;
    }

    private void RemoveLives()
    {
        foreach (var player in Players)
        {
            var players = GameObject.FindGameObjectsWithTag("Player").Select(s => s.GetComponent<PlayerNetworkBehavior>()).ToList();
            var objectPlayer = players.Where(p => p.netId.Value == player).FirstOrDefault();
            if (objectPlayer == null)
                return;
            int wins = 0;
            int guess = Guesses[player];
            if (Wins.ContainsKey(player))
                wins = Wins[player];
            if (wins != guess)
                objectPlayer.RpcRemoveLife();
        }
    }

    private void ResetDeck()
    {
        while (Table.Count > 0)
            Cards.Push(Table.Pop());
        ShuffleDeck();
    }

    public void StartGame()
    {
        if (GameStarted)
            return;
        StartRound();
        System.Random rnd = new System.Random();
        var position = rnd.Next(0, Players.Count - 1);
        ChangeCurrentPlayer(GetPlayerById(Players[position]));
        LastPlayer = GetPlayerById(Players.ElementAtOrDefault(position - 1)).Valid ? GetPlayerById(Players.ElementAtOrDefault(position - 1)) : GetPlayerById(Players.Last());
        GameStarted = true;
    }

    private void ChangeCurrentPlayer(Player player)
    {
        CurrentPlayer = player.Id;
        GetPlayerComponentById(CurrentPlayer).CanPlay = true;
    }

    public void StartRound()
    {
        SetGuessingRound();
        MaxRound = Players.Select(p => GetPlayerById(p).Lives).Max();
        ResetDeck();
        DistributeCards();
        MountWinsDictionary();
    }

    private void SetGuessingRound()
    {
        IsGuessing = true;
        var players = GetPlayersComponents();
        players.ForEach(p => p.GuessingRound = true);
    }

    private static List<PlayerNetworkBehavior> GetPlayersComponents()
    {
        return GameObject.FindGameObjectsWithTag("Player").Select(p => p.GetComponent<PlayerNetworkBehavior>()).ToList();
    }

    private static PlayerNetworkBehavior GetPlayerComponentById(uint id)
    {
        return GetPlayersComponents().Where(p => p.netId.Value == id).FirstOrDefault();
    }

    private void MountWinsDictionary()
    {
        Wins.Clear();
        foreach (var player in Players)
        {
            Wins[player] = 0;
        }
    }

    public bool Guess(Player player, int guess)
    {
        if (player == LastPlayer)
        {
            if (guess == MaxRound)
                return false;
            else
            {
                RemoveGuessingRound();
            }
        }
        Guesses[player.Id] = guess;

        ChangeCurrentPlayer(NextPlayer(player));
        return true;
    }

    private void RemoveGuessingRound()
    {
        IsGuessing = false;
        var players = GetPlayersComponents();
        players.ForEach(p => p.GuessingRound = false);
    }

    private Player NextPlayer(Player player)
    {
        return GetPlayerById(Players.ElementAtOrDefault(Players.IndexOf(player.Id) + 1)).Valid ? GetPlayerById(Players.ElementAtOrDefault(Players.IndexOf(player.Id) + 1)) 
            : GetPlayerById(Players.ElementAt(0));
    }

    private Player PreviousPlayer(Player player)
    {
        return GetPlayerById(Players.ElementAtOrDefault(Players.IndexOf(player.Id) - 1)).Valid ? GetPlayerById(Players.ElementAtOrDefault(Players.IndexOf(player.Id) - 1))
            : GetPlayerById(Players.Last()); ;
    }

    private void DistributeCards()
    {
        var cardsReceived = new Dictionary<uint, int>();
        Players.ForEach(p => cardsReceived[p] = 0);
        for (int i = 0; i < MaxRound; i++)
        {
            foreach (var player in Players)
            {
                if (cardsReceived[player] < GetPlayerById(player).Lives)
                {
                    GetPlayerComponentById(player).RpcAddCard(Cards.Pop());
                }
            }
        }
    }

    private void GenerateCards()
    {
        var cards = new List<Card>();
        for (int i = 0; i < 13; i++)
        {
            for (var j = Suit.CLUBS; j <= Suit.SPADES; j++)
            {
                cards.Add(new Card
                {
                    Value = i,
                    Suit = j,
                    Valid = true
                });
            }
        }
        while (cards.Count > 0)
        {
            System.Random random = new System.Random();
            var pos = random.Next(0, cards.Count - 1);
            Cards.Push(cards.ElementAt(pos));
            cards.RemoveAt(pos);
        }
    }

    private void ShuffleDeck()
    {
        var cards = Cards.ToList();
        Cards.Clear();
        while (cards.Count > 0)
        {
            System.Random random = new System.Random();
            var pos = random.Next(0, cards.Count - 1);
            Cards.Push(cards.ElementAt(pos));
            cards.RemoveAt(pos);
        }
    }

    private void GetAllPlayers()
    {
        var playersGO = GameObject.FindGameObjectsWithTag("Player");
        if (playersGO.Count() == PlayerCount)
            return;
        Players.Clear();
        foreach(var playerGO in playersGO)
        {
            var playerBehaviour = playerGO.GetComponent<PlayerNetworkBehavior>();
            Players.Add(playerBehaviour.Player.Id);
        }
        PlayerCount = playersGO.Count();
    }
}
