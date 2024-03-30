namespace IMDBApp.Models
{
    public class UpsertRequest
    {
        public string Name { get; set; }
        public string Details { get; set; }
        public string Type { get; set; }
        public int Rank { get; set; }
        public string Source { get; set; }
    }
}
