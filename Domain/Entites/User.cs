using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entites
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public required string UserName { get; set; }

        [Required]
        public required string HashPassword { get; set; }

        [Required]
        public required DateTime CreatedAt { get; set; }

        public DateTime? DisabledAt { get; set; }

        public Token? Token { get; set; }

        public ICollection<Todo> Todos { get; set; } = new List<Todo>();
    }
}
