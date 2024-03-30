using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using IMDBApp.Data;
using IMDBApp.Models;
using System.Net;
using IMDBApp.Dto;

namespace IMDBApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActorsController : ControllerBase
    {
        private readonly ApiContext _context;

        public ActorsController(ApiContext context)
        {
            _context = context;
        }

        // GET: api/Actors
        [HttpGet]
        public async Task<ActionResult<ActorsResponseDto>> GetActors(string actorName = null, int? minRank = null, int? maxRank = null, string provider = null, int skip = 0, int take = 20)
        {
            try
            {
                IQueryable<ActorModel> query = _context.Actors;
                // Apply filters
                if (!string.IsNullOrWhiteSpace(actorName))
                {
                    query = query.Where(actor => actor.Name.Contains(actorName));
                }

                if (minRank.HasValue)
                {
                    query = query.Where(actor => actor.Rank >= minRank);
                }

                if (maxRank.HasValue)
                {
                    query = query.Where(actor => actor.Rank <= maxRank);
                }

                if (!string.IsNullOrWhiteSpace(provider))
                {
                    query = query.Where(actor => actor.Source.Equals(provider, StringComparison.OrdinalIgnoreCase));
                }

                // Apply skip
                query = query.Skip(skip);

                // Apply take
                query = query.Take(take);

                var actors = await query.ToListAsync();
                // Map ActorModel instances to ActorModelDto instances containing only Id and Name as asked.
                var actorDtos = actors.Select(actor => new ActorModelDto { Id = actor.Id, Name = actor.Name }).ToArray();

                var response = new ActorsResponseDto
                {
                    Actors = actorDtos,     //The expected result should be actor (name+id) only.
                    Errors = null,
                    StatusCode = 200,
                    TraceId = Guid.NewGuid().ToString(),
                    IsSuccess = true
                };
                return Ok(response);
            }
            catch (Exception ex) { return CreateErrorResponse(HttpStatusCode.InternalServerError, "InternalServerError", ex.Message); }
        }

        // GET: api/Actors/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ActorResponse>> GetActor(string id)
        {
            var actorModel = await _context.Actors.FindAsync(id);
            if (actorModel == null)
            {
                return CreateErrorResponse(HttpStatusCode.NotFound, "NotFound", "Actor not found");
            }
            try
            {
                var actorResponse = new ActorResponse
                {
                    Errors = null,
                    StatusCode = 200,
                    TraceId = Guid.NewGuid().ToString(),
                    IsSuccess = true,
                    Actor = actorModel
                };
                return Ok(actorResponse);
            }
            catch (Exception ex) { return CreateErrorResponse(HttpStatusCode.InternalServerError, "InternalServerError", ex.Message); }
        }

        // DELETE: api/Actors/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Response>> DeleteActor(string id)
        {
            var actorModel = await _context.Actors.FindAsync(id);
            if (actorModel == null)
            {
                return CreateErrorResponse(HttpStatusCode.NotFound, "NotFound", "Actor not found");
            }
            try
            {
                _context.Actors.Remove(actorModel);
                await _context.SaveChangesAsync();

                var response = new Response
                {
                    Errors = null,
                    StatusCode = 200,
                    TraceId = Guid.NewGuid().ToString(),
                    IsSuccess = true
                };
                return Ok(response);
            }
            catch (Exception ex) { return CreateErrorResponse(HttpStatusCode.InternalServerError, "InternalServerError", ex.Message); }
        }

        // POST: api/Actors/5
        [HttpPost("{id}")]
        public async Task<ActionResult<ActorResponse>> UpdateActor(string id, UpsertRequest upsertRequest)
        {
            var actor = await _context.Actors.FindAsync(id);
            if (actor == null)
            {
                return CreateErrorResponse(HttpStatusCode.NotFound, "NotFound", "Actor not found");
            }

            // Check for duplicate rank
            if (await _context.Actors.AnyAsync(a => a.Rank == upsertRequest.Rank && a.Id != id))
            {
                return CreateErrorResponse(HttpStatusCode.Conflict, "Conflict", "A record with the same Rank already exists in the database.");
            }
            try
            {
                actor.Name = upsertRequest.Name;
                actor.Details = upsertRequest.Details;
                actor.Type = upsertRequest.Type;
                actor.Rank = upsertRequest.Rank;
                actor.Source = upsertRequest.Source;

                _context.Entry(actor).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                var actorResponse = new ActorResponse
                {
                    Errors = null,
                    StatusCode = 200,
                    TraceId = Guid.NewGuid().ToString(),
                    IsSuccess = true,
                    Actor = actor
                };
                return Ok(actorResponse);
            }
            catch(Exception ex) { return CreateErrorResponse(HttpStatusCode.InternalServerError, "InternalServerError", ex.Message); }
        }

        // POST: api/Actors
        [HttpPost]
        public async Task<ActionResult<ActorResponse>> AddActor(ActorModel actorModel)
        {
            // Check if the ID already exists in the database
            if (await _context.Actors.AnyAsync(a => a.Id == actorModel.Id))
            {
                return CreateErrorResponse(HttpStatusCode.Conflict, "Conflict", "A record with the same ID already exists in the database.");
            }

            // Check if the provided rank already exists in the database
            if (await _context.Actors.AnyAsync(a => a.Rank == actorModel.Rank))
            {
                return CreateErrorResponse(HttpStatusCode.Conflict, "Conflict", "A record with the same Rank already exists in the database.");
            }

            try
            {
                _context.Actors.Add(actorModel);
                await _context.SaveChangesAsync();

                var actorResponse = new ActorResponse
                {
                    Errors = null,
                    StatusCode = 200,
                    TraceId = Guid.NewGuid().ToString(),
                    IsSuccess = true,
                    Actor = actorModel
                };
                return Ok(actorResponse);
            }
            catch (Exception ex) { return CreateErrorResponse(HttpStatusCode.InternalServerError, "InternalServerError", ex.Message); }
        }

        //Error handling
        private ActionResult CreateErrorResponse(HttpStatusCode statusCode, string errorCode, string errorMessage)
        {
            var errorResponse = new Response
            {
                Errors = new Error[] { new Error { Code = errorCode, Message = errorMessage } },
                StatusCode = (int)statusCode,
                TraceId = Guid.NewGuid().ToString(),
                IsSuccess = false
            };
            return StatusCode((int)statusCode, errorResponse);
        }
    }
}