namespace HobbyAPI.Model
{
    public class Badge
    {
        public int Id { get; set; }
        public int habitid { get; set; }
        public string? clientId { get; set; }
        public string? name { get; set; }
        public int starter { get; set; }
        public int consistency { get; set; }
        public Badg3 badge { get; set; }
        public DateOnly date { get; set; }

    }
    public enum Badg3
    {
        IndexZero,
        Bronze, // Index One
        Silver,// Index Two
        Gold // index tree
    }
}
