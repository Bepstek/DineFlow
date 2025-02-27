using System.ComponentModel.DataAnnotations.Schema;
using dineflow.Models;

namespace dineflow.ViewModel
{
    public class MenuViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int CategoryId { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string? ExistingImage { get; set; }
        [NotMapped]
        public IFormFile? ImageFile { get; set; }
        public bool IsArchived { get; set; }
        public List<Category> Categories { get; set; }
    }
}
