using HtmlAgilityPack;
using IMDBApp.Data;
using IMDBApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApiContext>
    (opt => opt.UseInMemoryDatabase(builder.Configuration.GetConnectionString("actor")));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContext = services.GetRequiredService<ApiContext>();

    // Call my scraping funtion
    ScrapeAndSave(dbContext);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();


void ScrapeAndSave(ApiContext dbContext)
{
    try
    {
        // Initialize HtmlAgilityPack
        HtmlWeb web = new HtmlWeb();
        HtmlDocument document = web.Load("https://www.imdb.com/list/ls054840033/");

        // Scraping data from IMDb
        var actors = document.DocumentNode.SelectNodes("//div[@class='lister-item mode-detail']");
        var detailsAboutActor = document.DocumentNode.SelectNodes("//div[@class='list-description']//p");
        // Store scraped data into a list of ActorModel objects
        var actorList = new List<ActorModel>();

        int i = 1;
        foreach (var actor in actors)
        {
            var id = GenerateUniqueId(); // Generate unique identifier
            var name = actor.SelectSingleNode(".//h3[@class='lister-item-header']//a").InnerText;
            var typeNode = actor.SelectSingleNode(".//p[@class='text-muted text-small']");
            var type = typeNode != null ? typeNode.InnerText.Trim() : "";
            var details = "";           // Not all acotrs have a description
            try
            {
                details = detailsAboutActor.ToList()[i].InnerText;
                i++;
            }
            catch (Exception ex) { Console.WriteLine($"Error getting details: {ex.Message}"); }

            var rankNode = actor.SelectSingleNode(".//span[@class='lister-item-index unbold text-primary']").InnerText;
            var rank = rankNode.Substring(0, rankNode.IndexOf("."));
            var source = "IMDB";

            actorList.Add(new ActorModel
            {
                Id = id,
                Name = name.Substring(1, name.Length - 2),      //Cutting unnecessary characters
                Details = details,
                Type = type.Substring(0, type.IndexOf(" ")),    //Cutting unnecessary characters
                Rank = int.Parse(rank),                         // Rank defined as INT
                Source = source
            });
        }

        //Save data to in-memory database
        dbContext.Actors.AddRange(actorList);
        dbContext.SaveChanges();
    }
    catch (Exception ex) {Console.WriteLine($"An error occurred: {ex.Message}"); }  
}
string GenerateUniqueId()
{
    return Guid.NewGuid().ToString("N").Substring(0, 24);
}