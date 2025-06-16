using System.ComponentModel.DataAnnotations;

namespace Rospand_IMS.Models
{

    public class VendorQuickCreateModel
    {
        [Required]
        public string VendorType { get; set; } = "Business";

        [Required]
        public string CompanyName { get; set; }

        [Required]
        public string VendorDisplayName { get; set; }

        [EmailAddress]
        public string VendorEmail { get; set; }

        public string Mobile { get; set; }

        public int? CurrencyId { get; set; }

    }
    public class VendorCreateViewModel
    {
        // Basic Information
        [Required(ErrorMessage = "Vendor type is required")]
        [Display(Name = "Vendor Type")]
        public string VendorType { get; set; } // "Business" or "Individual"

        [Required(ErrorMessage = "Contact person name is required")]
        [Display(Name = "Contact Person")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string ContactPersonName { get; set; }

        [Display(Name = "Salutation")]
        [StringLength(10)]
        public string? Salutation { get; set; } // Mr., Mrs., Dr., etc.

        [Required(ErrorMessage = "First name is required")]
        [Display(Name = "First Name")]
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [Display(Name = "Last Name")]
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Company name is required")]
        [Display(Name = "Company Name")]
        [StringLength(100, ErrorMessage = "Company name cannot exceed 100 characters")]
        public string CompanyName { get; set; }

        [Required(ErrorMessage = "Display name is required")]
        [Display(Name = "Display Name")]
        [StringLength(100, ErrorMessage = "Display name cannot exceed 100 characters")]
        public string VendorDisplayName { get; set; }

        [Required(ErrorMessage = "Email or phone is required")]
        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string? VendorEmail { get; set; }

        [Display(Name = "Phone")]
        [Phone(ErrorMessage = "Invalid phone number")]
        public string? CustomerPhone { get; set; }

        [Display(Name = "Work Phone")]
        [Phone(ErrorMessage = "Invalid phone number")]
        public string? WorkPhone { get; set; }

        [Display(Name = "Mobile")]
        [Phone(ErrorMessage = "Invalid phone number")]
        public string? Mobile { get; set; }

        // Tax and Financial Information
        [Display(Name = "Tax Type")]
        public int? TaxTypeId { get; set; }

        [Display(Name = "Tax Registration Number")]
        [StringLength(50, ErrorMessage = "TRN cannot exceed 50 characters")]
        public string? TRNNumber { get; set; }

        [Display(Name = "Payment Terms")]
        public int? PaymentTermId { get; set; }

        [Display(Name = "Currency")]
        public int? CurrencyId { get; set; }

        [Display(Name = "Opening Balance")]
        [Range(0, double.MaxValue, ErrorMessage = "Opening balance cannot be negative")]
        public decimal? OpeningBalance { get; set; }

        // Address Information
        [Display(Name = "Shipping same as billing")]
        public bool ShippingSameAsBilling { get; set; } = true;

        public AddressViewModel? BillingAddress { get; set; }
        public AddressViewModel? ShippingAddress { get; set; }
    }

    public class AddressViewModel
    {
        [StringLength(100, ErrorMessage = "Attention cannot exceed 100 characters")]
        public string? Attention { get; set; }

        [Phone(ErrorMessage = "Invalid contact number")]
        public string? ContactNo { get; set; }

        [Display(Name = "Country")]
        public int? CountryId { get; set; }

        [Display(Name = "State/Province")]
        public int? StateId { get; set; }

        [Display(Name = "City")]
        public int? CityId { get; set; }

        [Display(Name = "Postal/Zip Code")]
        [StringLength(20, ErrorMessage = "Postal code cannot exceed 20 characters")]
        public string? ZipCode { get; set; }

        [Required(ErrorMessage = "Street address is required")]
        [Display(Name = "Street Address")]
        [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters")]
        public string StreetAddress { get; set; }
    }
}
