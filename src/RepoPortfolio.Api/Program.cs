using RepoPortfolio.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add RepoPortfolio services
var gitHubToken = builder.Configuration["GitHub:Token"] 
    ?? Environment.GetEnvironmentVariable("GITHUB_TOKEN");
builder.Services.AddRepoPortfolio(gitHubToken: gitHubToken);

// Add controllers
builder.Services.AddControllers();

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() 
    { 
        Title = "Repo Portfolio API", 
        Version = "v1",
        Description = "API for managing and scoring GitHub repository portfolios"
    });
});

var app = builder.Build();

// Initialize database with demo data
await app.Services.InitializeDatabaseAsync(seedDemoData: true);

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Repo Portfolio API v1");
        c.RoutePrefix = string.Empty; // Serve Swagger at root
    });
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
