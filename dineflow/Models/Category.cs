using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace dineflow.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
       
        public string Name { get; set; }

       

        public bool IsArchived { get; set; } = false;

        public virtual ICollection<Menu> Menus { get; set; }
    }
}
