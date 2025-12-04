using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebProgramlamaOdev.Models
{
    public class TrainerAvailability
    {
        [Key]
        public int AvailabilityId { get; set; }

        [Required]
        public int TrainerId { get; set; }

        [Required]
        [Display(Name = "Gün")]
        public DayOfWeek DayOfWeek { get; set; }

        [Required]
        [Display(Name = "Başlangıç Saati")]
        public TimeSpan StartTime { get; set; }

        [Required]
        [Display(Name = "Bitiş Saati")]
        public TimeSpan EndTime { get; set; }

        [Display(Name = "Aktif")]
        public bool IsActive { get; set; } = true;

        // Navigation Properties
        [ForeignKey("TrainerId")]
        public Trainer? Trainer { get; set; }
    }
}