using FdinhaServer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Util
{
    public static class GameStatus
    {
        public static bool IsDedicated = false;

        public static bool IsHost = false;

        public static ServerRoom ServerRoom = null;

        public const string ServerIp = "127.0.0.1";

        public static string PlayerName;
    }
}
