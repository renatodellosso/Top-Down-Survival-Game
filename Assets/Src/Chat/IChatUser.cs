using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Src.Chat
{
    public interface IChatUser
    {
        string Name { get; }

        virtual string FormatMessage(ChatMessage message)
        {
            return $"{message.Sender.Name}: {message.Text}";
        }
    }
}
