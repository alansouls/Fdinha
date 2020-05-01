using Assets.Scripts.Local;
using Assets.Scripts.Messages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor.VersionControl;

namespace Assets.Scripts.Util
{
    public class GameServer
    {
        private readonly UdpClient _udpClient;
        private int listenPort;

        public GameServer(int port)
        {
            listenPort = port;
            _udpClient = new UdpClient();
        }

        private void StartServer()
        {
            IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, listenPort);

            try
            {
                while (true)
                {
                    Console.WriteLine("Waiting for broadcast");
                    byte[] bytes = _udpClient.Receive(ref groupEP);
                    var json = Encoding.UTF8.GetString(bytes, 0, bytes.Length);

                    var message = JsonConvert.DeserializeObject<MessageModel>(json);
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
                listener.Close();
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
            }
        }

        public void AddPlayer(ActionObject action, IPEndPoint groupEP)
        {
            MatchController.AddPlayer(action.Player);
            var response = new ResponseMessage
            {
                Id = Guid.NewGuid(),
                CanPlay = false,
                GuessingRound = true,
                AdjustPlayer = false,
            };
            var json = JsonConvert.SerializeObject(response);
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            _udpClient.Send(bytes, bytes.Length, groupEP);
        }

        public void Guess(ActionObject action, IPEndPoint groupEP)
        {
            var response = MatchController.Guess(action.Player, action.Guess);
            var json = JsonConvert.SerializeObject(response);
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            _udpClient.Send(bytes, bytes.Length, groupEP);
        }

        public void Pass(ActionObject action, IPEndPoint groupEP)
        {
            var response = MatchController.Pass(action.Player);
            var json = JsonConvert.SerializeObject(response);
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            _udpClient.Send(bytes, bytes.Length, groupEP);
        }

        public void PlayCard(ActionObject action, IPEndPoint groupEP)
        {
            var response = MatchController.PlayCard(action.Player, action.Card);
            var json = JsonConvert.SerializeObject(response);
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            _udpClient.Send(bytes, bytes.Length, groupEP);
        }

        public MatchController MatchController { get; set; }
    }
}
