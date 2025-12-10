using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NuGet.DependencyResolver;

namespace WebProgramlamaOdev.Models
{
    public class Service
    {
        [Key]
        public int ServiceId { get; set; }

        [Required(ErrorMessage = "Hizmet adı zorunludur")]
        [StringLength(100)]
        [Display(Name = "Hizmet Adı")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Açıklama")]
        public string? Description { get; set; }

        [Required]
        [Display(Name = "Süre (Dakika)")]
        [Range(15, 180, ErrorMessage = "Süre 15-180 dakika arasında olmalıdır")]
        public int DurationMinutes { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Ücret")]
        [Range(0, 10000, ErrorMessage = "Ücret 0-10000 arasında olmalıdır")]
        public decimal Price { get; set; }

        // Foreign Key
        [Required]
        public int GymId { get; set; }

        // Navigation Properties
        [ForeignKey("GymId")]
        public Gym? Gym { get; set; }

        public ICollection<Trainer> Trainers { get; set; } = new List<Trainer>();

        [Display(Name = "Aktif")]
        public bool IsActive { get; set; } = true; // Varsayılan olarak aktif
    }

}