using System;
using System.ComponentModel.DataAnnotations;

namespace ChatRoulette.Repository.Model
{
    public class ChatConnection
    {
        [Key]
        public int Id { get; set; }
        public ChatConnectionResultEnum Result { get; set; }
        public DateTime DateCreated { get; set; }
        public string ExternalId { get; set; }

        public virtual ChatSession Session { get; set; }
    }
}