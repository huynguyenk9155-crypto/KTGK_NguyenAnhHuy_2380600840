using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace KTGK_LapTrinhWeb.Models
{
    public class Enrollment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public int CourseId { get; set; }

        [Required]
        public DateTime EnrollDate { get; set; } = DateTime.Now;

        [ForeignKey("UserId")]
        public virtual IdentityUser? User { get; set; }

        [ForeignKey("CourseId")]
        public virtual Course? Course { get; set; }
    }
}
