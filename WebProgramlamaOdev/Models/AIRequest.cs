using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace WebProgramlamaOdev.Models
{
    public class AIRequest
    {
        [Key]
        public int AIRequestId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Boy (cm)")]
        [Range(100, 250, ErrorMessage = "Boy 100-250 cm arasında olmalıdır")]
        public int Height { get; set; }

        [Required]
        [Display(Name = "Kilo (kg)")]
        [Range(30, 300, ErrorMessage = "Kilo 30-300 kg arasında olmalıdır")]
        public decimal Weight { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Vücut Tipi")]
        public string BodyType { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "Hedef")]
        public string Goal { get; set; } = string.Empty;

        [Display(Name = "Fotoğraf")]
        public string? PhotoPath { get; set; }

        [Display(Name = "AI Yanıtı")]
        public string? AIResponse { get; set; }

        [Display(Name = "Oluşturulma Tarihi")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "AI Görsel URL")]
        public string? GeneratedImageUrl { get; set; }

        // Navigation Properties
        [ForeignKey("UserId")]
        public IdentityUser? User { get; set; }
    }
}