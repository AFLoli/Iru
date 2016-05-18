using System.Collections.Generic;
using System.IO;
using SteamKit2;

namespace Iru_2
{
  internal abstract class Command
  {
    protected readonly SteamID ChatId;
    protected readonly SteamClient Client;
    protected readonly EChatEntryType Type;
    protected SteamID Chatter;
    protected string Message;
    protected string ComName;
    protected string[] Args;
    protected HashSet<string> AdminList;

    protected Command(SteamFriends.ChatMsgCallback callback, SteamClient steamClient)
    {
      ChatId = callback.ChatRoomID;
      Client = steamClient;
      Chatter = callback.ChatterID;
      Type = callback.ChatMsgType;
      Message = callback.Message;
      ComName = Message.Split(' ')[0];
    }

    public void Start()
    {
      if (GetPermission() && IsValid())
        Run();
      else
        Send("Command invalid");
    }

    protected abstract bool IsValid();

    protected abstract void Run();

    protected void Send(string message)
    {
      Client.GetHandler<SteamFriends>().SendChatRoomMessage(ChatId, Type, message);
    }

    protected string GetName(SteamID chatterId)
    {
      return Client.GetHandler<SteamFriends>().GetFriendPersonaName(chatterId);
    }

    protected bool GetPermission()
    {
      return !File.Exists($"perm/{ChatId.GetHashCode()}{ComName}.per");
    }
  }
}
