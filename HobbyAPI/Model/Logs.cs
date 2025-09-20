namespace HobbyAPI.Model
{
    public class Logs
    {
        public int Id { get; set; }
        public int HabitId { get; set; }
        public string? name { get; set; }
        public DateOnly date { get; set; }
        public GoalType goalType { get; set; }
        public int amount { get; set; }
        public string? clientId { get; set; }
    }
}
