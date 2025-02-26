using System.ComponentModel.DataAnnotations;

namespace dineflow.Models
{
    public class Menu

    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Category { get; set; }

        public string Description { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        public string ImageBase64 { get; set; } // This must match in the database

        public bool IsArchived { get; set; } = false;
    }

}
