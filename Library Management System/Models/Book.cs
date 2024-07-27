using System.ComponentModel.DataAnnotations;

namespace Library_Management_System.Models
{
    public class Book
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        [StringLength(100)]
        public string Author { get; set; }

        [Range(1000, 9999)]
        public int PublicationYear { get; set; }

        [Required]
        [StringLength(20)]
        public string ISBN { get; set; }
    }

}
