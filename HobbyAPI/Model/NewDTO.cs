using System.ComponentModel.DataAnnotations;

namespace HobbyAPI.Model
{
    public class NewDTO
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "O nome do habito é obrigatório.")]
        public string? name { get; set; }
        [Required]
        [RegularExpression("^(bool|count)$", ErrorMessage = "goalType deve ser 'bool' ou 'count'.")]
        public string? goalType { get; set; }
        public string? goal { get; set; }
    }
}
