using System.ComponentModel.DataAnnotations.Schema;

namespace ChatRoulette.Repository.Model
{
    [Table("ChatConnectionResults")]
    public class ChatConnectionResult : EnumBase<ChatConnectionResultEnum>
    {
    }
}