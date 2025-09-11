namespace HobbyAPI.Model
{
    public class Logs
    {
        public int Id { get; set; }
        public int HabitId { get; set; }
        public DateOnly date { get; set; }
        public int amount { get; set; }
    }
}
