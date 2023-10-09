using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Src.Chat
{
    internal class LocalChatUser : IChatUser
    {
        public string Name => "Local";

        string IChatUser.FormatMessage(ChatMessage message)
        {
            return $"[{Utils.FormatText(Name.ToUpper(), "green")}]: {message.Text}";
        }
    }
}
