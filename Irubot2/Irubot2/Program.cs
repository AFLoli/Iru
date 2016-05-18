using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using System.Threading;
using SteamKit2;

namespace Iru_2
{
    internal class Program
    {
        public static SteamClient SteamClient;
        private static SteamUser SteamUser;
        private static SteamFriends SteamFriends;
        private static string SteamUsername;
        private static string SteamPassword;
        private static bool _isRunning;
        private static HashSet<Room> _roomIDs;
        private const bool AyyyooooIsAFaggot = true;

        private static void Main(string[] args)
        {
            Console.WriteLine("Enter Username");
            SteamUsername = Console.ReadLine();
            Console.WriteLine("Enter Password");
            SteamPassword = Console.ReadLine();

            while (AyyyooooIsAFaggot)
            {
                SteamClient = new SteamClient(ProtocolType.Tcp);
                SteamUser = SteamClient.GetHandler<SteamUser>();
                SteamFriends = SteamClient.GetHandler<SteamFriends>();
                CallbackManager mgr = new CallbackManager(SteamClient);
                Callback<SteamClient.ConnectedCallback> callback1 = new Callback<SteamClient.ConnectedCallback>(OnConnected, mgr);
                Callback<SteamClient.DisconnectedCallback> callback2 = new Callback<SteamClient.DisconnectedCallback>(OnDisconnected, mgr);
                Callback<SteamUser.LoggedOnCallback> callback3 = new Callback<SteamUser.LoggedOnCallback>(OnLogon, mgr);
                Callback<SteamUser.AccountInfoCallback> callback4 = new Callback<SteamUser.AccountInfoCallback>(OnAccountInfo, mgr);
                Callback<SteamFriends.FriendsListCallback> callback5 = new Callback<SteamFriends.FriendsListCallback>(OnFriendsList, mgr);
                Callback<SteamFriends.ChatMsgCallback> callback6 = new Callback<SteamFriends.ChatMsgCallback>(OnChatMsg, mgr);
                Callback<SteamFriends.ChatInviteCallback> callback7 = new Callback<SteamFriends.ChatInviteCallback>(OnChatInvite, mgr);
                Callback<SteamFriends.ChatEnterCallback> callback8 = new Callback<SteamFriends.ChatEnterCallback>(OnChatEnter, mgr);
                Callback<SteamFriends.ChatMemberInfoCallback> callback9 = new Callback<SteamFriends.ChatMemberInfoCallback>(OnChatMemberInfo, mgr);
                SteamClient.Connect(null);
                _roomIDs=new HashSet<Room>();
                Console.WriteLine("Connecting");
                _isRunning = true;
                while (_isRunning)
                {
                    mgr.RunWaitCallbacks(System.TimeSpan.FromSeconds(1));

                    foreach (Room r in _roomIDs)
                    {
                        CheckGhost(r);
                    }
                }

               
            Console.WriteLine("Lost connection");
            }
        }

        private static void CheckGhost(Room roomId)
        {
            TimeSpan t = new TimeSpan(DateTime.Now.Ticks - roomId.LastMsgTime.Ticks);
            roomId.LastMsgTime = DateTime.Now;
            if (t.TotalMinutes > 15)
            {
                SteamFriends.JoinChat(new SteamID(ulong.Parse(roomId.RoomId)));
            }
        }

        private static void OnChatEnter(SteamFriends.ChatEnterCallback obj)
        {
        }

        private static void CommandWrapper(SteamFriends.ChatMsgCallback obj)
        {
            Command command = null;
            if (obj.Message.StartsWith("!hug"))
                command = new Hug(obj, SteamClient);
            if (obj.Message.StartsWith("!set"))
                command = new Set(obj, SteamClient);
            if (obj.Message.StartsWith("!memo"))
                command = new Memo(obj, SteamClient);
            if (command == null)
                return;
            new Thread(command.Start).Start();
        }

        private static void GetYTtitle(SteamFriends.ChatMsgCallback obj)
        {
            Match match = Regex.Match(obj.Message, "(http://|https://)?(www\\.)?(youtube|yimg|youtu)\\.([A-Za-z]{2,4}|[A-Za-z]{2}\\.[A-Za-z]{2})/(watch\\?v=)?[A-Za-z0-9\\-_]{6,12}(&[A-Za-z0-9\\-_]{1,}=[A-Za-z0-9\\-_]{1,})*");
            if (!match.Success)
                return;
            try
            {
                string request = ExecuteGetRequest(match.ToString());
                string str1 = request.Substring(request.IndexOf("<title>", StringComparison.Ordinal) + 7);
                string str2 = str1.Substring(0, str1.IndexOf("</title>", StringComparison.Ordinal)).Replace("&#39;", "'").Replace("&quot;", "\"").Replace("&amp;", "&");
                string message = $"YouTube: {str2.Remove(str2.LastIndexOf('-') - 1)}";
                SteamFriends.SendChatRoomMessage(obj.ChatRoomID, obj.ChatMsgType, message);
            }
            catch (Exception ex)
            {
                SteamFriends.SendChatRoomMessage(obj.ChatRoomID, obj.ChatMsgType, "Sorry I can't connect to Youtube for some reason ;_:");
            }
        }

        private static string ExecuteGetRequest(string url)
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            string str;
            using (HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse())
                str = new StreamReader(httpWebResponse.GetResponseStream()).ReadToEnd();
            httpWebRequest.Abort();
            return str;
        }

        private static void OnChatMsg(SteamFriends.ChatMsgCallback obj)
        {
            CommandWrapper(obj);
            GetYTtitle(obj);
        }

        private static void OnChatMemberInfo(SteamFriends.ChatMemberInfoCallback obj)
        {
            //When someone leaves
        }

        private static void OnChatInvite(SteamFriends.ChatInviteCallback obj)
        {
            SteamFriends.JoinChat(obj.ChatRoomID);
            _roomIDs.Add(new Room(obj.ChatRoomID.ToString()));
            try
            {
                using (FileStream fileStream = File.Open("Rooms.xml", FileMode.Truncate))
                    new BinaryFormatter().Serialize(fileStream, _roomIDs);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static void OnFriendsList(SteamFriends.FriendsListCallback obj)
        {
            foreach (SteamFriends.FriendsListCallback.Friend friend in obj.FriendList)
            {
                if (friend.Relationship == EFriendRelationship.RequestRecipient)
                    SteamFriends.AddFriend(friend.SteamID);
            }
            if (File.Exists("Rooms.xml"))
            {
                using (FileStream fileStream = File.Open("Rooms.xml", FileMode.Open))
                {
                    BinaryFormatter binaryFormatter = new BinaryFormatter();
                    try
                    {
                        _roomIDs = (HashSet<Room>)binaryFormatter.Deserialize(fileStream);
                    }
                    catch (SerializationException)
                    {
                        _roomIDs = new HashSet<Room>();
                    }
                }
            }
            else
            {
                File.Create("Rooms.xml");
                _roomIDs = new HashSet<Room>();
            }
            for (int i = 0; i < _roomIDs.Count; i++)
            {
                //_roomIDs.ToList()[i]
                SteamFriends.JoinChat(new SteamID(ulong.Parse(_roomIDs.ToList()[i].RoomId)));
                _roomIDs.ToArray()[i].LastMsgTime = DateTime.Now;
            }
            SteamFriends.SetPersonaName("Iru");
        }

        private static void OnAccountInfo(SteamUser.AccountInfoCallback obj)
        {
            SteamFriends.SetPersonaState(EPersonaState.Online);
        }

        private static void OnLogon(SteamUser.LoggedOnCallback obj)
        {
            Console.WriteLine("Trying to logon");
            if (obj.Result != EResult.OK)
            {
                Console.WriteLine("Log on failed.");
                Console.WriteLine(obj.ExtendedResult);
                _isRunning = false;
            }
            else
                Console.WriteLine("Logged on succesfully.");
        }

        private static void OnDisconnected(SteamClient.DisconnectedCallback obj)
        {
            Console.WriteLine("Disconnected from steam");
            _isRunning = false;
        }

        private static void OnConnected(SteamClient.ConnectedCallback obj)
        {
            if (obj.Result == EResult.OK)
            {
                Console.WriteLine("Connected, will try to log in");
                SteamUser.LogOn(new SteamUser.LogOnDetails
                {
                    Username = SteamUsername,
                    Password = SteamPassword
                });
            }
            else
            {
                Console.WriteLine("Connection failed");
                _isRunning = false;
            }
        }
    }
}
