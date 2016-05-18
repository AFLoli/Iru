using System.IO;
using System.Text.RegularExpressions;
using SteamKit2;

namespace Iru_2
{
  internal class Set : Command
  {
    public Set(SteamFriends.ChatMsgCallback callback, SteamClient steamClient)
      : base(callback, steamClient)
    {
    }

    protected override bool IsValid()
    {
        var match = Regex.Match(Message, "!set ?([!\\w]+) (true|false)");
        if (match.Groups.Count != 3)
        return false;
      Args = new string[2];
      Args[0] = match.Groups[1].Value;
      Args[1] = match.Groups[2].Value;
      return true;
    }

    protected override void Run()
    {
      if (!GetName(Chatter).Equals("Animal Fetish Loli"))
        return;
      string path = $"perm/{ChatId.GetHashCode()}{Args[0]}.per";
      if (Args[1].Equals("true"))
      {
        if (File.Exists(path))
          File.Delete(path);
        Send("I can now use " + Args[0]);
      }
      else
      {
        if (!File.Exists(path))
        {
          if (!Directory.Exists("perm"))
            Directory.CreateDirectory("perm");
          File.Create(path).Close();
        }
        Send("I can't " + Args[0] + " anymore");
      }
    }
  }
}
