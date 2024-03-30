using Microsoft.AspNetCore.Http;
using System;

namespace IMDBApp.Models
{
    public class ActorResponse
    {
        public Error[] Errors { get; set; }
        public int StatusCode { get; set; }
        public string TraceId { get; set; }
        public bool IsSuccess { get; set; }
        public ActorModel Actor { get; set; }


    }
}
