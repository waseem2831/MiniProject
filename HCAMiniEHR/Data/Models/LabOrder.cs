using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HCAMiniEHR.Data.Models
{
    [Table("LabOrder", Schema = "Healthcare")]
    public class LabOrder
    {
        [Key]
        public int LabOrderId { get; set; }

        [Required]
        public int AppointmentId { get; set; }

        [Required]
        [StringLength(100)]
        public string TestName { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, InProgress, Completed

        public DateTime OrderDate { get; set; } = DateTime.Now;

        public DateTime? CompletedDate { get; set; }

        [StringLength(500)]
        public string? Results { get; set; }

        [StringLength(200)]
        public string? Notes { get; set; }

        // Navigation Properties
        [ForeignKey("AppointmentId")]
        public virtual Appointment Appointment { get; set; } = null!;
    }
}