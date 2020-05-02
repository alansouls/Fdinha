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
        
        public IPAddress ServerIp { get; set; }
        public PlayerBehavior Player { get; set; }
        public IPEndPoint ServerEP;

        public GameClient(int port, int serverPort)
        {
            this.serverPort = serverPort;
            _udpClient = new UdpClient(port);
        }

        public void ListenServerUpdates()
        {
            Player.GameClientThread  = new Thread(new ThreadStart(() => { ListenServerUpdatesThread(); }));
            Player.GameClientThread.Start();
        }

        private void ListenServerUpdatesThread()
        {
            ServerEP = new IPEndPoint(ServerIp, serverPort);
            try
            {
                while (true)
                {
                    Console.WriteLine("Waiting for broadcast");
                    byte[] bytes = _udpClient.Receive(ref ServerEP);
                    var json = Encoding.UTF8.GetString(bytes, 0, bytes.Length);

                    var message = JsonUtility.FromJson<ResponseMessage>(json);
                    HandleMessage(message, ServerEP);
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

        private void HandleMessage(ResponseMessage message, IPEndPoint groupEP)
        {
            Player.Guesses = message.Guesses;
            Player.Wins = message.Wins;
            Player.Table = message.Table;
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
