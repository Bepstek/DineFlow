using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dineflow.Models
{
    public class Inventory
    {
        [Key]
        public int InventoryId { get; set; }

        [Required]
        public DateTime DateOfInventory { get; set; }

        [Required]
        public string UserId { get; set; } // Reference ApplicationUser

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        public string Status { get; set; }

        public virtual ICollection<InventoryDetails> InventoryDetails { get; set; } = new List<InventoryDetails>();
    }
}
