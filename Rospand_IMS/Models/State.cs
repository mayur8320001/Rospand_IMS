using System.ComponentModel.DataAnnotations;

namespace Rospand_IMS.Models
{
    public class State
    {
        public int Id { get; set; }

        [Required]
        public string StateId { get; set; } // e.g., "MH"

        public string Name { get; set; }

        public int CountryId { get; set; }
        public virtual Country? Country { get; set; }

        public virtual ICollection<City> Cities { get; set; } = new List<City>();
    }
    public class ExcelUploadViewModel
    {
        [Required]
        public IFormFile ExcelFile { get; set; }
    }


}