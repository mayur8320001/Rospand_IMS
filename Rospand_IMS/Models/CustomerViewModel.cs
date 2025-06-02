using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Rospand_IMS.Models
{
    public class RequiredWhenAttribute : ValidationAttribute
    {
        private readonly string _propertyName;
        private readonly object _desiredValue;

        public RequiredWhenAttribute(string propertyName, object desiredValue)
        {
            _propertyName = propertyName;
            _desiredValue = desiredValue;
        }

        protected override ValidationResult IsValid(object value, ValidationContext context)
        {
            var instance = context.ObjectInstance;
            var type = instance.GetType();
            var propertyValue = type.GetProperty(_propertyName)?.GetValue(instance, null);

            if (propertyValue?.ToString() == _desiredValue.ToString() && value == null)
            {
                return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;
        }
    }
    public class CustomerViewModel
    {
        public int Id { get; set; }

        // Basic Information
        [Required(ErrorMessage = "Customer type is required")]
        public string CustomerType { get; set; }

        public string? Salutation { get; set; }

        [RequiredWhen("CustomerType", "Individual")]
        public string? FirstName { get; set; }

        [RequiredWhen("CustomerType", "Individual")]
        public string? LastName { get; set; }

        [RequiredWhen("CustomerType", "Business")]
        public string? ContactPersonName { get; set; }

        [RequiredWhen("CustomerType", "Business")]
        public string? CompanyName { get; set; }

        [Required]
        public string CustomerDisplayName { get; set; }

        [EmailAddress]
        public string? CustomerEmail { get; set; }

        [Phone]
        public string? WorkPhone { get; set; }

        [Phone]
        public string? Mobile { get; set; }

        // Tax Information
        public int? TaxTypeId { get; set; }
        public string? TRNNumber { get; set; }

        // Financial Information
        [Required]
        public int PaymentTermId { get; set; }

        [Required]
        public int CurrencyId { get; set; }

        public decimal OpeningBalance { get; set; } = 0;

        // Billing Address
        public string? BillingAttention { get; set; }
        public string? BillingContactNo { get; set; }
        public int? BillingCountryId { get; set; }
        public int? BillingStateId { get; set; }
        public int? BillingCityId { get; set; }
        public string? BillingZipCode { get; set; }
        public string? BillingStreetAddress { get; set; }

        // Shipping Address
        public bool ShippingSameAsBilling { get; set; } = true;
        public string? ShippingAttention { get; set; }
        public string? ShippingContactNo { get; set; }
        public int? ShippingCountryId { get; set; }
        public int? ShippingStateId { get; set; }
        public int? ShippingCityId { get; set; }
        public string? ShippingZipCode { get; set; }
        public string? ShippingStreetAddress { get; set; }

        // Dropdown lists
        public SelectList? CustomerTypes { get; set; }
        public SelectList? TaxTypes { get; set; }
        public SelectList? PaymentTerms { get; set; }
        public SelectList? Currencies { get; set; }
        public SelectList? Countries { get; set; }
        public SelectList? States { get; set; }
        public SelectList? Cities { get; set; }
    }
}
