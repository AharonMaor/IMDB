using IMDBApp.Models;

namespace IMDBApp.Dto
{
    public class ActorsResponseDto
    {
        public Error[] Errors { get; set; }
        public int StatusCode { get; set; }
        public string TraceId { get; set; }
        public bool IsSuccess { get; set; }
        public ActorModelDto[] Actors { get; set; }
    }
}
