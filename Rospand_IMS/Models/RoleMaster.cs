using System.ComponentModel.DataAnnotations;

namespace Rospand_IMS.Models
{
    public class RoleMaster
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string? RoleName { get; set; }

        // Navigation property
        public List<PageAccess> PageAccesses { get; set; } = new List<PageAccess>();
    }
}