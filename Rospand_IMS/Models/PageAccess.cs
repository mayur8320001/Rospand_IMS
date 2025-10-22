using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rospand_IMS.Models
{
    public class PageAccess
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int RoleId { get; set; }

        [Required]
        [StringLength(100)]
        public string? PageName { get; set; }

        public bool IsAdd { get; set; } = false;
        public bool IsEdit { get; set; } = false;
        public bool IsDelete { get; set; } = false;

        // Navigation property
        [ForeignKey("RoleId")]
        public RoleMaster? Role { get; set; }
    }
}