using System.ComponentModel.DataAnnotations;

namespace WebProgramlamaOdev.Models
{
    public class AiExerciseRequest
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Height is required")]
        [Range(100, 250, ErrorMessage = "Height must be between 100 and 250")]
        public int Height { get; set; }

        [Required(ErrorMessage = "Weight is required")]
        [Range(30, 200, ErrorMessage = "Weight must be between 30 and 200")]
        public int Weight { get; set; }

        [Required(ErrorMessage = "Goal is required")]
        public string Goal { get; set; }

       
        public string? UserId { get; set; }

       
        public string? AiResponse { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
