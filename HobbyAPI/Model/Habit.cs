namespace HobbyAPI.Model
{
    public class Habit
    {
        public int Id { get; set; }
        public string? name { get; set; }
        public GoalType goalType { get; set; }
        public int goal { get; set; }
    }
    public enum GoalType
    {
        Inicio,
        Bool,
        Count
    }
}
