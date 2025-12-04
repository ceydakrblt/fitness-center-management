using System.ComponentModel.DataAnnotations;
using NuGet.DependencyResolver;

namespace WebProgramlamaOdev.Models
{
    public class Gym
    {
        [Key]
        public int GymId { get; set; }

        [Required(ErrorMessage = "Salon adı zorunludur")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(200)]
        public string Address { get; set; } = string.Empty;

        [Phone]
        public string? PhoneNumber { get; set; }

        [Required]
        [Display(Name = "Açılış Saati")]
        public TimeSpan OpeningTime { get; set; }

        [Required]
        [Display(Name = "Kapanış Saati")]
        public TimeSpan ClosingTime { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        // Navigation Properties
        public ICollection<Service> Services { get; set; } = new List<Service>();
        public ICollection<Trainer> Trainers { get; set; } = new List<Trainer>();
    }
}