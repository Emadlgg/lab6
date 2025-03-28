namespace LaLigaTrackerBackend.Models
{
    public class Match
    {
        public int Id { get; set; }
        public string HomeTeam { get; set; } = string.Empty;
        public string AwayTeam { get; set; } = string.Empty;
        public string MatchDate { get; set; } = string.Empty;
        public int Goals { get; set; } = 0;
        public int YellowCards { get; set; } = 0;
        public int RedCards { get; set; } = 0;
        public int ExtraTimeMinutes { get; set; } = 0;
    }
}