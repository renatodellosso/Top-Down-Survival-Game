using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;

#nullable enable
namespace Assets.Src.Chat
{
    public class ChatMessage : IFormattable, INetworkSerializable
    {
        
        static readonly ServerChatUser SystemChatUser = new();

        private string text;
        public string Text { 
            get
            {
                return text;
            }
            private set
            {
                text = value;
            }
        }

        public IChatUser Sender { get; protected set; }

        //For deserialization
        public ChatMessage()
        {
            text = "";
            Sender = SystemChatUser;
        }

        public ChatMessage(IChatUser sender, string text)
        {
            this.text = text;
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

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref text);
        }
    }
}
