using System;
using SteamKit2;

namespace Iru_2
{
    [Serializable]
    public struct Room
    {
        public Room(string roomId) : this()
        {
            this.RoomId = roomId;
            LastMsgTime = DateTime.Now;
            
        }

        public DateTime LastMsgTime { get; set; }

        public string RoomId { get; set; }
    }
}