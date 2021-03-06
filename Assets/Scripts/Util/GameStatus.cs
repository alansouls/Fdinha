using FdinhaServer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Util
{
    public static class GameStatus
    {
        public static bool IsDedicated = false;

        public static bool IsHost = false;

        public static ServerRoom ServerRoom = null;

        public const string ServerHostName = "servercsgoclub01.brazilsouth.cloudapp.azure.com";

        public static string PlayerName;

        public static IPAddress ServerIP 
        {
            get
            {
                return Dns.GetHostEntry(ServerHostName).AddressList.FirstOrDefault();
            } 
        }
    }
}
