using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dineflow.Models
{
    public class TransactionDetail
    {
        [Key]
        public int TransactionDetailsId { get; set; }

        // Foreign Key for Transaction
        public int TransactionId { get; set; }
        [ForeignKey("TransactionId")]
        public virtual Transaction Transaction { get; set; }

        // Foreign Key for MenuItem
        public int MenuItemId { get; set; }
        [ForeignKey("MenuItemId")]
        public virtual Menu MenuItem { get; set; }

        public int Quantity { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Total { get; set; }
    }
}
