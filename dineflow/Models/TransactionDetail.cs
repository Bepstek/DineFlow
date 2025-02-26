using System;
using System.ComponentModel.DataAnnotations;

namespace dineflow.Models
{
    public class TransactionDetail
    {
        [Key]
        public int TransactionDetailsId { get; set; }
        public int TransactionId { get; set; }
        public int MenuItemId { get; set; }
        public int Quantity { get; set; }
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }
        [Range(0, double.MaxValue)]
        public decimal Total { get; set; }
        public Transaction Transaction { get; set; }
        public virtual Menu MenuItem { get; set; }
    }

}
