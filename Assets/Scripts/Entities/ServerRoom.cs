using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace FdinhaServer.Entities
{
    [Serializable]
    public class ServerRoom
    {
        public string Name;

        public string Password;

        public bool Open;
    }

    [Serializable]
    public class ServerList
    {
        [SerializeField]
        public ServerRoom[] ServerRooms;
    }
}
