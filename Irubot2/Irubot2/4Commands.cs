using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using SteamKit2;

namespace Iru_2
{
  internal class Memo : Command
  {
    public Memo(SteamFriends.ChatMsgCallback callback, SteamClient steamClient)
      : base(callback, steamClient)
    {
    }

    protected override bool IsValid()
    {
      Match match = Regex.Match(Message, "!memo (\\S+)(.*)");
      if (match.Groups.Count != 3)
        return false;
      Args = new string[2];
      Args[0] = match.Groups[1].Value.Trim();
      Args[1] = match.Groups[2].Value.Trim();
      return true;
    }

    protected override void Run()
    {
      if (Args[1].Equals(""))
      {
        string path = $"memo/m{Args[0].GetHashCode()}.txt";
        if (!File.Exists(path))
        {
          Send("I don't have a memo like that.");
        }
        else
        {
          MemoStruct memoStruct;
          using (FileStream fileStream = File.Open(path, FileMode.Open))
            memoStruct = (MemoStruct) new BinaryFormatter().Deserialize(fileStream);
          Send(memoStruct.ToString());
        }
      }
      else
      {
        MemoStruct memoStruct = new MemoStruct(Args[0], Args[1], Chatter);
        if (!Directory.Exists("memo"))
          Directory.CreateDirectory("memo");
        using (FileStream fileStream = File.Create($"memo/m{memoStruct.Name.GetHashCode()}.txt"))
          new BinaryFormatter().Serialize(fileStream, memoStruct);
        Send("Ok, I got it!");
      }
    }

    [Serializable]
    private class MemoStruct
    {
      public string Name;
      public string Text;
      public DateTime CreatedOn;
      public string AuthorId;

      public MemoStruct(string name, string text, SteamID authorId)
      {
        Name = name;
        Text = text;
        AuthorId = authorId.ToString();
        CreatedOn = DateTime.Now;
      }

      public override string ToString()
      {
        return $"Memo {Name}: {Text}";
      }
    }
  }
}
