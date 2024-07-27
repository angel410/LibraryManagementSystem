using System.ComponentModel.DataAnnotations;

namespace Library_Management_System.Models
{
    public class Patron
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(200)]
        [EmailAddress]
        public string ContactInformation { get; set; }
    }
}
