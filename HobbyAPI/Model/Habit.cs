using System.ComponentModel.DataAnnotations;

namespace HobbyAPI.Model
{
    public class Habit
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "O nome do habito é obrigatório.")]
        public string? name { get; set; }
        [Required]
        [RegularExpression("^(bool|count)$", ErrorMessage = "goalType deve ser 'bool' ou 'count'.")]
        public GoalType goalType { get; set; }
        public int goal { get; set; }
        public  DateOnly createdAt { get; set; }
        public DateOnly updatedAt { get; set; }
        [Required(ErrorMessage = "Sua NameTag é obrigatória")]
        public string? clientId { get; set; }
    }
    public enum GoalType
    {
        IndexZero,
        Bool, // Index One
        Count // Index Two
    }
}
