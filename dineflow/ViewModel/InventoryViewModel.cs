using System.ComponentModel.DataAnnotations.Schema;
using dineflow.Models;
namespace dineflow.ViewModel
{
    public class InventoryViewModel
    {
        public int InventoryId { get; set; }
        public int MenuItemId { get; set; }
        public List<Menu> MenuItems { get; set; } 
        
    }

}
