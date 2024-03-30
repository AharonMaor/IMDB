namespace IMDBApp.Models
{
    public class ActorsResponse
    {
        public Error[] Errors { get; set; }
        public int StatusCode { get; set; }
        public string TraceId { get; set; }
        public bool IsSuccess { get; set; }
        public ActorModel[] Actors { get; set; }
    }
}
