using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entites
{
    public class Token
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public required string RefreshToken { get; set; }

        [Required]
        public required DateTime CreatedAt { get; set; }

        [Required]
        public required DateTime ExpiresAt { get; set; }

        public DateTime? RevokedAt { get; set; }

        [ForeignKey("ApplicationUser")]
        public int UserId { get; set; }

        public User ApplicationUser { get; set; } = null!;
    }
}