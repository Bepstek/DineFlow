using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dineflow.Models
{
    public class InventoryDetails
    {
        [Key]
        public int InventoryDetailsId { get; set; }

        [Required]
        public int InventoryId { get; set; }

        [ForeignKey("InventoryId")]
        public virtual Inventory Inventory { get; set; }

        [Required]
        public string ItemName { get; set; }

        

        public string UnitOfMeasure { get; set; }

        [Required]
        public int Quantity { get; set; }
    }
}
