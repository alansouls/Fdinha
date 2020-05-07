using Assets.Scripts.Entites;
using Assets.Scripts.Enums;
using Assets.Scripts.Local;
using Assets.Scripts.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class MatchController : MonoBehaviour
{
    public Stack<Card> Cards;
    public List<Player> Players;
    public int MaxRound;
    public Stack<Card> Table;
    public Player WinningPlayer;
    public Player CurrentPlayer;
    public bool IsGuessing;
    public Player LastPlayer;
    public IDictionary<Player, int> Guesses;
    public IDictionary<Player, int> Wins;
    public GameServer GameServer;
    public List<Card> CardsGone;

    // Start is called before the first frame update
    void Start()
    {
        GameServer = new GameServer(8965);
        GameServer.MatchController = this;
        GameServer.StartServer();
        Table = new Stack<Card>();
        Cards = new Stack<Card>();
        Players = new List<Player>();
        Wins = new Dictionary<Player, int>();
        Guesses = new Dictionary<Player, int>();
        MaxRound = 3;
        IsGuessing = false;
        GenerateCards();
        //GetAllPlayers();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddPlayer(Player player)
    {
        Players.Add(player);
    }

    public void OnDestroy()
    {
        GameServer.Close();
    }

    public void PlayCard(Player player, Card card)
    {
        var newPlayer = Players.Where(p => p.Id == player.Id).FirstOrDefault();
        newPlayer.Cards = player.Cards;
        Players.Remove(newPlayer);
        Players.Add(newPlayer);
        
        if (player == LastPlayer)
        {
            PlaceCardInTable(player, card);
            AddWinToWinningPlayer();
            CurrentPlayer = new Player();
            GameServer.UpdateGameState();
            Thread.Sleep(500);
            CardsGone = Table.ToList();
            Table.Clear();
            GameServer.UpdateGameState();
            Thread.Sleep(500);
            CurrentPlayer = WinningPlayer;
            LastPlayer = PreviousPlayer(CurrentPlayer);
            if (!Players.Where(p => p.Cards.Count > 0).Any())
            {
                RemoveLives();
                StartRound();
            }
            GameServer.UpdateGameState();
            Thread.Sleep(500);
        }
        else
        {
            PlaceCardInTable(player, card);
            CurrentPlayer = NextPlayer(player);
            GameServer.UpdateGameState();
            Thread.Sleep(500);
        }
        if (Players.Where(p => p.Lives > 0).Count() <= 1)
        {
            EndGame();
            GameServer.UpdateGameState();
            Thread.Sleep(500);
        }
    }

    public void Pass(Player player)
    {
        if (player.Cards.Count <= 0)
            CurrentPlayer = NextPlayer(player);
        else
            CurrentPlayer = player;
        GameServer.UpdateGameState();
    }

    private void EndGame()
    {
        if (Players.Any())
        {
            Debug.Log($"We have a winner!!! {Players.First().Name} won the game");
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
            WinningPlayer = player;
        }
        else
        {
            var topCard = Table.Peek();
            if (topCard < card)
            {
                Table.Push(card);
                WinningPlayer = player;
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
        CurrentPlayer = WinningPlayer;
        LastPlayer = PreviousPlayer(WinningPlayer);
    }

    private void AddWinToWinningPlayer()
    {
        if (Wins.ContainsKey(WinningPlayer))
            Wins[WinningPlayer] += 1;
    }

    private void RemoveLives()
    {
        foreach (var player in Players.ToList())
        {
            var newPlayer = player;
            int wins = 0;
            int guess = Guesses[player];
            if (Wins.ContainsKey(player))
                wins = Wins[player];
            if (wins != guess)
            {
                newPlayer.Lives -= 1;
                Players.Remove(newPlayer);
                Players.Add(newPlayer);
            }
        }
    }

    private void ResetDeck()
    {
        while (Table.Count > 0)
            Cards.Push(Table.Pop());
        foreach(var card in CardsGone)
        {
            Cards.Push(card);
        }
        CardsGone.Clear();
        ShuffleDeck();
    }

    public void StartGame()
    {
        StartRound();
        System.Random rnd = new System.Random();
        var position = rnd.Next(0, Players.Count - 1);
        CurrentPlayer = Players[position];
        LastPlayer = Players.ElementAtOrDefault(position - 1).Valid ? Players.ElementAtOrDefault(position - 1) : Players.Last();
        GameServer.UpdateGameState();
    }

    public void StartRound()
    {
        IsGuessing = true;
        CurrentPlayer = WinningPlayer;
        LastPlayer = PreviousPlayer(CurrentPlayer);
        MaxRound = Players.Select(p => p.Lives).Max();
        ResetDeck();
        DistributeCards();
        MountWinsDictionary();
    }

    private void MountWinsDictionary()
    {
        Wins.Clear();
        Guesses.Clear();
        foreach (var player in Players)
        {
            Wins[player] = 0;
            Guesses[player] = 0;
        }
    }

    public void ZeroGuessesAndWinnins()
    {
        foreach (var player in Guesses.Keys)
        {
            Guesses[player] = 0;
        }
    }

    public void Guess(Player player, int guess)
    {
        if (player == LastPlayer)
        {
            var sumGuesses = Guesses.Select(g => g.Value).Sum();
            if (sumGuesses + guess == MaxRound)
            {
                GameServer.UpdateGameState();
                return;
            }
            else
            {
                IsGuessing = false;
            }
        }
        Guesses[player] = guess;
        CurrentPlayer = NextPlayer(player);
        GameServer.UpdateGameState();
    }

    private Player NextPlayer(Player player)
    {
        return Players.ElementAtOrDefault(Players.IndexOf(player) + 1).Valid ? Players.ElementAtOrDefault(Players.IndexOf(player) + 1) : Players.ElementAt(0);
    }

    private Player PreviousPlayer(Player player)
    {
        return Players.ElementAtOrDefault(Players.IndexOf(player) - 1).Valid ? Players.ElementAtOrDefault(Players.IndexOf(player) - 1) : Players.Last();
    }

    private void DistributeCards()
    {
        var cardsReceived = new Dictionary<Player, int>();
        Players.ForEach(p => cardsReceived[p] = 0);
        foreach (var player in Players)
        {
            int cardsGiven = 0;
            while (cardsGiven < player.Lives)
            {
                player.Cards.Add(Cards.Pop());
                cardsGiven++;
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
        foreach(var playerGO in playersGO)
        {
            var playerBehaviour = playerGO.GetComponent<PlayerBehavior>();
            Players.Add(playerBehaviour.Player);
        }
    }
}
