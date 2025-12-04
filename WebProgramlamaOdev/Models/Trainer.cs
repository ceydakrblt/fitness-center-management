using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebProgramlamaOdev.Models
{
    public class Trainer
    {
        [Key]
        public int TrainerId { get; set; }

        [Required(ErrorMessage = "Ad Soyad zorunludur")]
        [StringLength(100)]
        [Display(Name = "Ad Soyad")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "E-posta")]
        public string Email { get; set; } = string.Empty;

        [Phone]
        [Display(Name = "Telefon")]
        public string? PhoneNumber { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Uzmanlık Alanları")]
        public string Specialization { get; set; } = string.Empty;

        [StringLength(1000)]
        [Display(Name = "Biyografi")]
        public string? Biography { get; set; }

        [Display(Name = "Fotoğraf URL")]
        public string? PhotoUrl { get; set; }

        // Foreign Key
        [Required]
        public int GymId { get; set; }

        // Navigation Properties
        [ForeignKey("GymId")]
        public Gym? Gym { get; set; }

        public ICollection<Service> Services { get; set; } = new List<Service>();
        public ICollection<TrainerAvailability> Availabilities { get; set; } = new List<TrainerAvailability>();
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}