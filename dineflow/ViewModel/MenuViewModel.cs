
using System.ComponentModel.DataAnnotations.Schema;

namespace dineflow.ViewModel
{
    public class MenuViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        [NotMapped]
        public IFormFile ImageFile { get; set; } 
    }

}
