using System.ComponentModel.DataAnnotations;

namespace Rospand_IMS.Models
{
    public class Tax
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [Range(0, 100)]
        public decimal Rate { get; set; } // Tax rate in percentage

        [StringLength(10)]
        public string Code { get; set; } // Tax code like GST, VAT, etc.
        [StringLength(500)]
        public string Description { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedDate { get; set; }
    }
}
