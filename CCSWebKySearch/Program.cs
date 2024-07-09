using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using CCSWebKySearch.Models;
using CCSWebKySearch.Services;
using AutoMapper;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<MyDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
                     new MySqlServerVersion(new Version(8, 0, 21))));

builder.Services.AddScoped<INotebookService, NotebookService>();
builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddScoped<ICheckLiveService, CheckLiveService>();

// Configure Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API v1"));
}

app.UseHttpsRedirection();


// CheckLive endpoint
app.MapGet("/checklive", (ICheckLiveService checkLiveService) =>
{
    return Results.Ok(new { live = checkLiveService.IsLive() });
})
.WithName("CheckLive")
.WithOpenApi();

// Notebooks endpoint
app.MapGet("/notebooks", async (INotebookService notebookService, IMapper mapper) =>
{
    var notebooks = await notebookService.GetAllNotebooksAsync();
    var notebooksDto = mapper.Map<IEnumerable<NotebookDto>>(notebooks);
    return Results.Ok(notebooksDto);
})
.WithName("GetAllNotebooks")
.WithMetadata(new HttpMethodMetadata(new[] { "GET" }));

app.Run();
