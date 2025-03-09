using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dineflow.Models
{
    public class Menu
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        // Foreign Key for Category
        [Required]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; }

        public string Description { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

       

        public byte[] ImageBytes { get; set; }
        public bool IsArchived { get; set; } = false;
    }
}
