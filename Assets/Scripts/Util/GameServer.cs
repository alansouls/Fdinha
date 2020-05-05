using Assets.Scripts.Entities;
using Assets.Scripts.Local;
using Assets.Scripts.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

namespace Assets.Scripts.Util
{
    public class GameServer
    {
        private readonly UdpClient _udpClient;
        private readonly int listenPort;

        public GameServer(int port)
        {
            listenPort = port;
            _udpClient = new UdpClient(port);
            PlayersIps = new Dictionary<Player, IPEndPoint>();
            MessagesRead = new List<string>();
        }

        public void StartServer()
        {
            try
            {
                _udpClient.BeginReceive(new AsyncCallback((a) => MessageReceived(a)), null);
            }
            catch (SocketException e)
            {
                Debug.Log(e.Message);
            }
        }

        private void MessageReceived(IAsyncResult a)
        {
            IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, 0);
            var bytes = _udpClient.EndReceive(a, ref groupEP);
            var json = Encoding.UTF8.GetString(bytes, 0, bytes.Length);

            var message = JsonUtility.FromJson<MessageModel>(json);
            if (!MessagesRead.Contains(message.MessageId))
                HandleMessage(message, groupEP);
            StartServer();
        }

        public void HandleMessage(MessageModel message, IPEndPoint groupEP)
        {
            MessagesRead.Add(message.MessageId);
            var action = message.Action;
            switch (message.Action.Action)
            {
                case Enums.Action.ADD_PLAYER:
                    AddPlayer(action, groupEP);
                    break;
                case Enums.Action.GUESS:
                    Guess(action, groupEP);
                    break;
                case Enums.Action.PASS:
                    Pass(action, groupEP);
                    break;
                case Enums.Action.PLAY_CARD:
                    PlayCard(action, groupEP);
                    break;
                case Enums.Action.START_GAME:
                    StartGame(action, groupEP);
                    break;
            }
        }

        private void StartGame(ActionObject action, IPEndPoint groupEP)
        {
            MatchController.StartGame();
        }

        public void AddPlayer(ActionObject action, IPEndPoint groupEP)
        {
            MatchController.AddPlayer(action.Player);
            PlayersIps.Add(action.Player, groupEP);
            var response = new ResponseMessage
            {
                Id = Guid.NewGuid().ToString(),
                CanPlay = false,
                GuessingRound = true,
                AdjustPlayer = false,
            };
            var json = JsonUtility.ToJson(response);
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            _udpClient.Send(bytes, bytes.Length, groupEP);
        }

        public void Guess(ActionObject action, IPEndPoint groupEP)
        {
            if (action.Player != MatchController.CurrentPlayer)
                return;
            MatchController.Guess(action.Player, action.Guess);
        }

        public void Pass(ActionObject action, IPEndPoint groupEP)
        {
            if (action.Player != MatchController.CurrentPlayer)
                return;
            MatchController.Pass(action.Player);
        }

        public void PlayCard(ActionObject action, IPEndPoint groupEP)
        {
            if (action.Player != MatchController.CurrentPlayer)
                return;
            MatchController.PlayCard(action.Player, action.Card);
        }

        public void UpdateGameState()
        {
            foreach (var p in PlayersIps.Keys)
            {
                var message = new ResponseMessage
                {
                    Id = Guid.NewGuid().ToString(),
                    GameStates = GameState.MountGameStates(MatchController.Guesses, MatchController.Wins),
                    GuessingRound = MatchController.IsGuessing,
                    Table = MatchController.Table.ToList(),
                    CanPlay = p == MatchController.CurrentPlayer,
                    AdjustPlayer = true,
                    Player = MatchController.Players.Where(x => x.Id == p.Id).FirstOrDefault()
                };
                var bytes = GetMessageBytes(message);
                _udpClient.Send(bytes, bytes.Length, PlayersIps[p]);
            }
        }

        public void SendPlayerUpdate(Player player)
        {
            var message = new ResponseMessage
            {
                Id = Guid.NewGuid().ToString(),
                AdjustPlayer = true,
                Player = player
            };
            var bytes = GetMessageBytes(message);
            _udpClient.Send(bytes, bytes.Length, PlayersIps[player]);
        }

        private static byte[] GetMessageBytes(ResponseMessage response)
        {
            var json = JsonUtility.ToJson(response);
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            return bytes;
        }

        public MatchController MatchController { get; set; }
        public Dictionary<Player, IPEndPoint> PlayersIps;
        public List<string> MessagesRead { get; set; }
    }
}
