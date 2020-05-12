using Assets.Scripts.Extensions;
using FdinhaServer.Entities;
using FdinhaServer.Messages;
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
        public List<string> MessagesRead { get; set; }

        public GameClient(int port, int serverPort)
        {
            this.serverPort = serverPort;
            _udpClient = new UdpClient(port);
            MessagesRead = new List<string>();
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
            if (!MessagesRead.Contains(message.Id))
                HandleMessage(message);
            ListenServerUpdates();
        }

        private void HandleMessage(ResponseMessage message)
        {
            MessagesRead.Add(message.Id);
            Player.Guesses = GameState.GuessesDictionary(message.GameStates);
            Player.Wins = GameState.WinsDictionary(message.GameStates);
            Player.Players = message.Players;
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

        public List<ServerRoom> GetServerRooms()
        {
            var msgStr = "GET_ROOMS";
            var msgBytes = Encoding.UTF8.GetBytes(msgStr);
            _udpClient.Send(msgBytes, msgBytes.Length, ServerEP);
            var bytes = _udpClient.Receive(ref ServerEP);
            var json = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
            var rooms = JsonUtility.FromJson<ServerList>(json).ServerRooms;
            return rooms.ToList();
        }

        public void Close()
        {
            _udpClient.Close();
        }
    }
}
