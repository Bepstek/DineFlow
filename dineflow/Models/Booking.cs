using System;
using System.ComponentModel.DataAnnotations;

namespace dineflow.Models
{
    public class Booking
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PhoneNumber { get; set; }

        [Required]
        public int NumberOfPeople { get; set; }

        [Required]
        public DateTime DateTime { get; set; }
    }
}
