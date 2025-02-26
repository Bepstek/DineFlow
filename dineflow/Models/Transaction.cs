using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace dineflow.Models
{
    public class Transaction
    {
        [Key]   
        
        public int TransactionId { get; set; }
        
        public int ReservationId { get; set; }
        public int TableId { get; set; }
        public int UserId { get; set; }
        public DateTime OrderDate { get; set; }
        [Range(0, double.MaxValue)]
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } // Enum as string

        public ICollection<TransactionDetail> TransactionDetail { get; set; } = new List<TransactionDetail>();
    }

}
