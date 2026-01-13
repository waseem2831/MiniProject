using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HCAMiniEHR.Data.Models
{
    [Table("Appointment", Schema = "Healthcare")]
    public class Appointment
    {
        [Key]
        public int AppointmentId { get; set; }

        [Required]
        public int PatientId { get; set; }

        [Required]
        public DateTime AppointmentDate { get; set; }

        [Required]
        [StringLength(50)]
        public string AppointmentType { get; set; } = string.Empty; // Checkup, Follow-up, Emergency

        [Required]
        [StringLength(100)]
        public string DoctorName { get; set; } = string.Empty;

        [StringLength(20)]
        public string Status { get; set; } = "Scheduled"; // Scheduled, Completed, Cancelled

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation Properties
        [ForeignKey("PatientId")]
        public virtual Patient? Patient { get; set; }

        public virtual ICollection<LabOrder> LabOrders { get; set; } = new List<LabOrder>();
    }
}
