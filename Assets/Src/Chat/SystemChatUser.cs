using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Src.Chat
{
    internal class SystemChatUser : IChatUser
    {
        public string Name => "System";

        string IChatUser.FormatMessage(ChatMessage message)
        {
            return $"[{Utils.FormatText(Name.ToUpper(), "yellow")}]: {message.Text}";
        }
    }
}
