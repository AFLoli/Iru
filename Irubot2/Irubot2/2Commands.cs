using SteamKit2;

namespace Iru_2
{
  internal class Hug : Command
  {
    public Hug(SteamFriends.ChatMsgCallback callback, SteamClient steamClient)
      : base(callback, steamClient)
    {
    }

    protected override bool IsValid()
    {
      return Message.StartsWith("!hug");
    }

    protected override void Run()
    {
      Send($"*Hugs {GetName(Chatter)}*");
    }
  }
}
