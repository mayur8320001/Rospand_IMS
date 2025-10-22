using System.ComponentModel.DataAnnotations;

namespace Rospand_IMS.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string? Username { get; set; }

        [Required]
        public string? PasswordHash { get; set; }

        [Required]
        [StringLength(50)]
        public string? Role { get; set; }

        [StringLength(100)]
        public string? Email { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? LastLoginDate { get; set; }
    }
}