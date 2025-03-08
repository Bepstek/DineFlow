using System;
using System.ComponentModel.DataAnnotations;

namespace dineflow.Models
{
    public class ActivityLog
    {
        [Key]
        public int LogId { get; set; }

        [Required]
        public string UserId { get; set; } // ID of the user performing the action

        [Required]
        public string Action { get; set; } // Description of the action (e.g., "Created Order")

        [Required]
        public string Details { get; set; } // Additional details about the action

        [Required]
        public DateTime Timestamp { get; set; } // Time when the action was performed
    }
}