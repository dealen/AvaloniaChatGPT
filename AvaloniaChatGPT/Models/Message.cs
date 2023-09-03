using System;

namespace AvaloniaChatGPT.Models
{
    public class Message
    {
        public Guid Id { get; set; }
        public string ChatMessage { get; set; }
        public MessageType Type { get; set; }
        public DateTime Time { get; set; }
    }
}
