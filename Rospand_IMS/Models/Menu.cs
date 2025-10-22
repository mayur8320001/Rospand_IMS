using System.ComponentModel.DataAnnotations;

namespace Rospand_IMS.Models
{
    public class Menu
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string? Name { get; set; }

        [Required]
        [StringLength(100)]
        public string? ControllerName { get; set; }

        [Required]
        [StringLength(100)]
        public string? ActionName { get; set; }

        [StringLength(50)]
        public string? IconClass { get; set; }

        public int? ParentId { get; set; }

        public int DisplayOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;
    }
}