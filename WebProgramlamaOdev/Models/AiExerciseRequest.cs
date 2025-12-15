
using System;
using System.ComponentModel.DataAnnotations;
namespace WebProgramlamaOdev.Models
{
    public class AiExerciseRequest
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public int Height { get; set; }

        [Required]
        public int Weight { get; set; }

        [Required]
        public string Goal { get; set; }

        public string AiResponse { get; set; }

        public DateTime CreatedAt { get; set; }
    }

}
