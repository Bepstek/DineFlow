using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dineflow.Models
{
    public class Transaction
    {
        [Key]
        public int TransactionId { get; set; }

        public int? ReservationId { get; set; } // Allow null for walk-in customers

        public int TableId { get; set; }
        public string OrderId { get; set; }

        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }
        public DateTime OrderDate { get; set; }

        [Range(0, double.MaxValue)] 
        public decimal TotalAmount { get; set; }

        public string Status { get; set; } // Enum as string

        public ICollection<TransactionDetail> TransactionDetails { get; set; } = new List<TransactionDetail>();
    }
}
