using System.ComponentModel.DataAnnotations;

namespace Rospand_IMS.Models.Account
{
    public class Ledger
    {
        public int LedgerId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string LedgerName { get; set; }   // e.g., Cash, Bank, Sales, Rent Expense
        
        [Required]
        [StringLength(50)]
        public string LedgerType { get; set; }   // Asset, Liability, Income, Expense
        
        [Required]
        public decimal OpeningBalance { get; set; }
        
        [Required]
        public decimal CurrentBalance { get; set; }
        
        [Required]
        public DateTime CreatedDate { get; set; }
        
        // Navigation Properties
        public virtual ICollection<Transaction> LedgerTransactions { get; set; } = new List<Transaction>();
    }

}