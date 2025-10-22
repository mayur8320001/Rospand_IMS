namespace Rospand_IMS.Models.Account
{
    public class DailyExpense
    {
        public int ExpenseId { get; set; }
        public string ExpenseType { get; set; }   // e.g., Electricity, Rent, Transport
        public decimal Amount { get; set; }
        public string PaymentMode { get; set; }   // Cash, Bank, UPI
        public DateTime ExpenseDate { get; set; }
        public string ApprovedBy { get; set; }
    }

}
