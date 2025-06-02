using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Rospand_IMS.Models
{
    public class Customer
    {
        public int Id { get; set; }

        // Basic Information
        [Required(ErrorMessage = "Customer type is required")]
        [Display(Name = "Customer Type")]
        public string CustomerType { get; set; } // "Business" or "Individual"

        [Display(Name = "Salutation")]
        [StringLength(10)]
        public string? Salutation { get; set; }

        [RequiredWhen(nameof(CustomerType), "Individual", ErrorMessage = "First name is required for individual customers")]
        [Display(Name = "First Name")]
        [StringLength(50)]
        public string? FirstName { get; set; }

        [RequiredWhen(nameof(CustomerType), "Individual", ErrorMessage = "Last name is required for individual customers")]
        [Display(Name = "Last Name")]
        [StringLength(50)]
        public string? LastName { get; set; }

        [RequiredWhen(nameof(CustomerType), "Business", ErrorMessage = "Contact person name is required for business customers")]
        [Display(Name = "Contact Person")]
        [StringLength(100)]
        public string? ContactPersonName { get; set; }

        [RequiredWhen(nameof(CustomerType), "Business", ErrorMessage = "Company name is required for business customers")]
        [Display(Name = "Company Name")]
        [StringLength(100)]
        public string? CompanyName { get; set; }

        [Required(ErrorMessage = "Display name is required")]
        [Display(Name = "Display Name")]
        [StringLength(100)]
        public string CustomerDisplayName { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Email")]
        [StringLength(100)]
        public string? CustomerEmail { get; set; }

        [Phone(ErrorMessage = "Invalid phone number")]
        [Display(Name = "Work Phone")]
        [StringLength(20)]
        public string? WorkPhone { get; set; }

        [Phone(ErrorMessage = "Invalid mobile number")]
        [Display(Name = "Mobile")]
        [StringLength(20)]
        public string? Mobile { get; set; }

        // Tax Information
        [Display(Name = "Tax Type")]
        public int? TaxTypeId { get; set; }
        public TaxType? TaxType { get; set; }

        [Display(Name = "TRN Number")]
        [StringLength(50)]
        public string? TRNNumber { get; set; }

        // Financial Information
        [Required(ErrorMessage = "Payment term is required")]
        [Display(Name = "Payment Term")]
        public int PaymentTermId { get; set; }
        public PaymentTerm? PaymentTerm { get; set; }

        [Required(ErrorMessage = "Currency is required")]
        [Display(Name = "Currency")]
        public int CurrencyId { get; set; }
        public Currency? Currency { get; set; }

        [Display(Name = "Opening Balance")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal OpeningBalance { get; set; } = 0;

        // Addresses
        [Display(Name = "Billing Address")]
        public int? BillingAddressId { get; set; }
        public Address? BillingAddress { get; set; }

        [Display(Name = "Shipping Address")]
        public int? ShippingAddressId { get; set; }
        public Address? ShippingAddress { get; set; }

        [Display(Name = "Same as billing address")]
        public bool ShippingSameAsBilling { get; set; } = true;

        // Additional fields
        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Display(Name = "Modified Date")]
        public DateTime? ModifiedDate { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

  
    }
}