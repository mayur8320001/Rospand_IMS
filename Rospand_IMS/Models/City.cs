namespace Rospand_IMS.Models
{
    public class City
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public int StateId { get; set; } // Foreign Key to State table
        public string? StateCode { get; set; } // Optional: for display or search

        public virtual State? State { get; set; }
    }
}
