
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rospand_IMS.Models
{
    public class Vendor
    {
        public int Id { get; set; }

        // Basic Information (Mandatory)
        [Required]
        public string VendorType { get; set; } // Business or Individual
        [Required]
        public string ContactPersonName { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public string CompanyName { get; set; }
        [Required]
        public string VendorDisplayName { get; set; }
        [Required]
        [EmailAddress]
        public string? VendorEmail { get; set; }
        public string? Salutation { get; set; } // Optional

        public string? CustomerPhone { get; set; } // Optional
        public string? WorkPhone { get; set; } // Optional
        public string? Mobile { get; set; } // Optional

        // Tax and Financial Information (Optional)
        public int? TaxTypeId { get; set; }
        public string? TRNNumber { get; set; }
        public int? PaymentTermId { get; set; }
        public int? CurrencyId { get; set; }
        public decimal? OpeningBalance { get; set; }

        // Address Information (Optional)
        public int? BillingAddressId { get; set; }
        public Address? BillingAddress { get; set; }
        public bool ShippingSameAsBilling { get; set; }
        public int? ShippingAddressId { get; set; }
        public Address? ShippingAddress { get; set; }

        // Navigation properties
        public TaxType? TaxType { get; set; }
        public PaymentTerm? PaymentTerm { get; set; }
        public Currency? Currency { get; set; }


        // Purchase-related properties
        [Column(TypeName = "decimal(18,4)")]
        public decimal TotalPurchases { get; set; }
        public DateTime? LastPurchaseDate { get; set; }
        public int PaymentTermDays { get; set; } = 30;
        public bool IsActive { get; set; } = true;
        public string? Notes { get; set; }
      
        

        // Navigation properties
        public ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();
    }
    public enum Salutation
    {
        [Display(Name = "Mr.")]
        Mr,

        [Display(Name = "Mrs.")]
        Mrs,

        [Display(Name = "Ms.")]
        Ms,

        [Display(Name = "Dr.")]
        Dr,

        [Display(Name = "Prof.")]
        Pro
    }
}