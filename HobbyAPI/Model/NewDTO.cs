using System.ComponentModel.DataAnnotations;

namespace HobbyAPI.Model
{
    public class NewDTO
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Você deve botar nomear o hábito")]
        public string? name { get; set; }
        [Required]
        [RegularExpression("^(bool|count)$", ErrorMessage = "goalType deve ser 'bool' ou 'count'.")]
        public string? goalType { get; set; }
        public string? goal { get; set; }
        public DateOnly createdAt { get; set; }
        public DateOnly updatedAt { get; set; }
      //  public string? clientId { get; set; }
    }
}
