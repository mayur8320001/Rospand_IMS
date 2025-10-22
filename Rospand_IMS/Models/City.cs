using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rospand_IMS.Models
{
    public class City
    {
        public int Id { get; set; }
        public string Name { get; set; }

         [Required]
        [ForeignKey("State")]
        public string StateId { get; set; }
        [ForeignKey(nameof(StateId))]
        public virtual State? State { get; set; }
    }
}
