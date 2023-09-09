using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable
namespace Assets.Src.Chat
{
    public class ChatMessage : IFormattable
    {
        
        static readonly SystemChatUser SystemChatUser = new();

        public string Text { get; protected set; }
        public IChatUser Sender { get; protected set; }

        public ChatMessage(IChatUser sender, string text)
        {
            Text = text;
            Sender = sender;
        }

        public ChatMessage(string text) : this(SystemChatUser, text) { }

        public override string ToString()
        {
            return Sender.FormatMessage(this);
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return ToString();
        }
    }
}
