using Microsoft.OpenApi.Models;
using CCSWebKySearch.Models;
using CCSWebKySearch.Services;
using AutoMapper;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) => lc.WriteTo.Console().ReadFrom.Configuration(ctx.Configuration));

builder.Services.AddScoped<INotebookService, NotebookService>();
builder.Services.AddAutoMapper(typeof(Program));

// CORS configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", b => b.AllowAnyHeader()
                                        .AllowAnyOrigin()
                                        .AllowAnyMethod());

});

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
app.UseCors("AllowAll");
app.UseHttpsRedirection();

// CheckLive endpoint
app.MapGet("/checklive", (ICheckLiveService checkLiveService) =>
{
    return Results.Ok(new { live = checkLiveService.IsLive() });
})
.WithName("CheckLive")
.WithOpenApi();

// Notebooks endpoint
app.MapGet("/notebooks", async (INotebookService notebookService, IMapper mapper, int? count) =>
{
    try
    {
        var notebooks = await notebookService.GetAllNotebooksAsync(count ?? 500);
        var notebooksDto = mapper.Map<IEnumerable<NotebookDto>>(notebooks);
        return Results.Ok(notebooksDto);
    }
    catch(Exception)
    {
        return Results.BadRequest("invalid input, count should be > 0, <= 1000");
    }

})
.WithName("GetAllNotebooks")
.WithMetadata(new HttpMethodMetadata(new[] { "GET" }));

app.Run();
