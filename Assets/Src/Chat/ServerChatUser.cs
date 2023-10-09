using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Src.Chat
{
    internal class ServerChatUser : IChatUser
    {
        public string Name => "Server";

        string IChatUser.FormatMessage(ChatMessage message)
        {
            return $"[{Utils.FormatText(Name.ToUpper(), "yellow")}]: {message.Text}";
        }
    }
}
