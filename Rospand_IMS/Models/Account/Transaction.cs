namespace Rospand_IMS.Models.Account
{
    public class Transaction
    {
        public int TransactionId { get; set; }
        public int LedgerId { get; set; }
        public virtual Ledger Ledger { get; set; }

        public decimal Amount { get; set; }
        public bool IsDebit { get; set; }  // true = Debit, false = Credit
        public string Narration { get; set; }
        public DateTime TransactionDate { get; set; }
    }

}
