namespace HobbyAPI.Model
{
    public class Habit
    {
        public int Id { get; set; }
        public string? name { get; set; }
        public GoalType goalType { get; set; }
        public int goal { get; set; }
        public  DateOnly createdAt { get; set; }
        public DateOnly updatedAt { get; set; }
      //  public string? clientId { get; set; }
    }
    public enum GoalType
    {
        IndexZero,
        Bool, // Index One
        Count // Index Two
    }
}
