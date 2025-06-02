using System.ComponentModel.DataAnnotations;

namespace Rospand_IMS.Models
{
    public class Unit
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [Required]
        [StringLength(10)]
        public string Symbol { get; set; }

        [StringLength(200)]
        public string Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
