using Microsoft.EntityFrameworkCore;
using IMDBApp.Models;

namespace IMDBApp.Data
{
    public class ApiContext : DbContext
    {
        public DbSet<ActorModel> Actors { get; set; }

        public ApiContext(DbContextOptions<ApiContext> options)
            : base(options)
        {

        }
    }
}
