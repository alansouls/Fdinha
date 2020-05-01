using Assets.Scripts.Local;
using Assets.Scripts.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor.VersionControl;
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
            _udpClient = new UdpClient();
        }

        public void StartServer()
        {
            var thread = new Thread(new ThreadStart(() => { StartServerThread(); }));
            thread.Start();
        }

        private void StartServerThread()
        {
            IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, listenPort);

            try
            {
                while (true)
                {
                    Console.WriteLine("Waiting for broadcast");
                    byte[] bytes = _udpClient.Receive(ref groupEP);
                    var json = Encoding.UTF8.GetString(bytes, 0, bytes.Length);

                    var message = JsonUtility.FromJson<MessageModel>(json);
                    var thread = new Thread(new ThreadStart(() => { HandleMessage(message, groupEP); }));
                    thread.Start();
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                _udpClient.Close();
                Debug.Log("Server closed");
            }
        }

        public void HandleMessage(MessageModel message, IPEndPoint groupEP)
        {
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
            UpdateGameState();
        }

        private void StartGame(ActionObject action, IPEndPoint groupEP)
        {
            MatchController.StartGame();
        }

        public void AddPlayer(ActionObject action, IPEndPoint groupEP)
        {
            MatchController.AddPlayer(action.Player);
            playersIps.Add(action.Player, groupEP);
            var response = new ResponseMessage
            {
                Id = Guid.NewGuid(),
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
            var response = MatchController.Guess(action.Player, action.Guess);
            var json = JsonUtility.ToJson(response);
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            _udpClient.Send(bytes, bytes.Length, groupEP);
        }

        public void Pass(ActionObject action, IPEndPoint groupEP)
        {
            var response = MatchController.Pass(action.Player);
            var json = JsonUtility.ToJson(response);
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            _udpClient.Send(bytes, bytes.Length, groupEP);
        }

        public void PlayCard(ActionObject action, IPEndPoint groupEP)
        {
            MatchController.PlayCard(action.Player, action.Card);
        }

        public void UpdateGameState()
        {
            
            playersIps.Keys.ToList().ForEach(p =>
            {
                var message = new ResponseMessage
                {
                    Id = Guid.NewGuid(),
                    Guesses = MatchController.Guesses,
                    GuessingRound = MatchController.IsGuessing,
                    Wins = MatchController.Wins,
                    Table = MatchController.Table,
                    CanPlay = p == MatchController.CurrentPlayer,
                    AdjustPlayer = true,
                    Player = MatchController.Players.Where(x => x.Id == p.Id).FirstOrDefault()
                };
                var bytes = GetMessageBytes(message);
                _udpClient.Send(bytes, bytes.Length, playersIps[p]);
            });
        }

        public void SendPlayerUpdate(Player player)
        {
            var message = new ResponseMessage
            {
                Id = Guid.NewGuid(),
                AdjustPlayer = true,
                Player = player
            };
            var bytes = GetMessageBytes(message);
            _udpClient.Send(bytes, bytes.Length, playersIps[player]);
        }

        private static byte[] GetMessageBytes(ResponseMessage response)
        {
            var json = JsonUtility.ToJson(response);
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            return bytes;
        }

        public MatchController MatchController { get; set; }
        public Dictionary<Player, IPEndPoint> playersIps;
    }
}
