using Assets.Scripts.Entities;
using Assets.Scripts.Extensions;
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
using UnityEngine;
using UnityEngine.XR;

namespace Assets.Scripts.Util
{
    public class GameClient
    {
        private readonly UdpClient _udpClient;
        private readonly int serverPort;
        public PlayerBehavior Player { get; set; }
        public IPEndPoint ServerEP;

        public GameClient(int port, int serverPort)
        {
            this.serverPort = serverPort;
            _udpClient = new UdpClient(port);
        }

        public void DefineServerEP(IPAddress serverIp)
        {
            ServerEP = new IPEndPoint(serverIp, serverPort);
        }

        public void ListenServerUpdates()
        {
            try
            {
                _udpClient.BeginReceive(new AsyncCallback((a) => ServerMessageReceived(a)), null);
            }
            catch (SocketException e)
            {
                Console.WriteLine(e);
            }
        }

        public void ServerMessageReceived(IAsyncResult a)
        {
            var bytes = _udpClient.EndReceive(a, ref ServerEP);
            var json = Encoding.UTF8.GetString(bytes, 0, bytes.Length);

            var message = JsonUtility.FromJson<ResponseMessage>(json);
            HandleMessage(message);
            ListenServerUpdates();
        }

        private void HandleMessage(ResponseMessage message)
        {
            Player.Guesses = GameState.GuessesDictionary(message.GameStates);
            Player.Wins = GameState.WinsDictionary(message.GameStates);
            Player.Table = message.Table.ToStack();
            Player.CanPlay = message.CanPlay;
            Player.GuessingRound = message.GuessingRound;
            if (message.AdjustPlayer)
                Player.Player = message.Player;
        }

        public void SendCommandToServer(MessageModel message)
        {
            var bytes = GetMessageBytes(message);
            _udpClient.Send(bytes, bytes.Length, ServerEP);
        }

        private static byte[] GetMessageBytes(MessageModel message)
        {
            var json = JsonUtility.ToJson(message);
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            return bytes;
        }
    }
}
