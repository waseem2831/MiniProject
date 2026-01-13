using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HCAMiniEHR.Data.Models
{
    [Table("AuditLog", Schema = "Healthcare")]
    public class AuditLog
    {
        [Key]
        public int AuditLogId { get; set; }

        [Required]
        [StringLength(50)]
        public string TableName { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string Operation { get; set; } = string.Empty; // INSERT, UPDATE, DELETE

        public int RecordId { get; set; }

        [StringLength(1000)]
        public string? OldValues { get; set; }

        [StringLength(1000)]
        public string? NewValues { get; set; }

        public DateTime AuditDate { get; set; } = DateTime.Now;

        [StringLength(100)]
        public string? UserName { get; set; }
    }
}