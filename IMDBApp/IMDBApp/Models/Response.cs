namespace IMDBApp.Models
{
    public class Response
    {
        public Error[] Errors { get; set; }
        public int StatusCode { get; set; }
        public string TraceId { get; set; }
        public bool IsSuccess { get; set; }
    }
}
