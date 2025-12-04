using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace WebProgramlamaOdev.Models
{
    public class Appointment
    {
        [Key]
        public int AppointmentId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public int TrainerId { get; set; }

        [Required]
        public int ServiceId { get; set; }

        [Required]
        [Display(Name = "Randevu Tarihi")]
        public DateTime AppointmentDate { get; set; }

        [Required]
        [Display(Name = "Başlangıç Saati")]
        public TimeSpan StartTime { get; set; }

        [Required]
        [Display(Name = "Bitiş Saati")]
        public TimeSpan EndTime { get; set; }

        [Display(Name = "Durum")]
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;

        [StringLength(500)]
        [Display(Name = "Notlar")]
        public string? Notes { get; set; }

        [Display(Name = "Oluşturulma Tarihi")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Toplam Ücret")]
        public decimal TotalPrice { get; set; }

        // Navigation Properties
        [ForeignKey("UserId")]
        public IdentityUser? User { get; set; }

        [ForeignKey("TrainerId")]
        public Trainer? Trainer { get; set; }

        [ForeignKey("ServiceId")]
        public Service? Service { get; set; }
    }

    public enum AppointmentStatus
    {
        [Display(Name = "Beklemede")]
        Pending = 0,

        [Display(Name = "Onaylandı")]
        Approved = 1,

        [Display(Name = "Tamamlandı")]
        Completed = 2,

        [Display(Name = "İptal Edildi")]
        Cancelled = 3
    }
}