using System.ComponentModel.DataAnnotations;

namespace Rospand_IMS.Models
{
    public class PaymentTerm
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; } = "";

        [Required]
        public int? NetDays { get; set; }// Number of days until payment is due

        public bool IsActive { get; set; } = true;

        public DateTime? CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedDate { get; set; }
    }
}
