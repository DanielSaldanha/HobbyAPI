namespace HobbyAPI.Model
{
    public class DTOLogs
    {
        public int Id { get; set; }
        public int HabitId { get; set; }
        public string? name { get; set; }
        public DateOnly date { get; set; }
        public string? goalType { get; set; }
        public string? amount { get; set; }
    }
}
