using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatRoulette.Repository.Model
{
    public class EnumBase<TEnum> where TEnum : struct
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public virtual int Id { get; set; }

        [Required] [MaxLength(100)]
        public virtual string Name { get; set; }

        [MaxLength(100)] public virtual string Description { get; set; }
    }
}