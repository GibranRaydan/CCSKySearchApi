using Microsoft.OpenApi.Models;
using CCSWebKySearch.Models;
using CCSWebKySearch.Services;
using AutoMapper;
using Serilog;
using Microsoft.AspNetCore.Mvc;
using DotNetEnv;
using CCSWebKySearch.Dtos;
using CCSWebKySearch.Contracts;

var builder = WebApplication.CreateBuilder(args);

// Load the .env file
Env.Load();

// Retrieve values from .env
var connectionString = Env.GetString("CONNECTION_STRING");
var documentsPath = Env.GetString("DOCUMENTS_PATH");
var seqServerUrl = Env.GetString("SEQ_SERVER_URL");

// Set up configuration to override values with those from .env
builder.Configuration.AddInMemoryCollection(new Dictionary<string, string>
{
    { "ConnectionStrings:DefaultConnection", connectionString },
    { "DocumentsPath", documentsPath },
    { "Serilog:WriteTo:2:Args:serverUrl", seqServerUrl }
});


builder.Host.UseSerilog((ctx, lc) => lc.WriteTo.Console().ReadFrom.Configuration(ctx.Configuration));

builder.Services.AddScoped<INotebookService, NotebookService>();
builder.Services.AddScoped<ILandSearchPageBookService, BookPageSearchService>();
builder.Services.AddScoped<IKindSearchService, KindSearchService>();
builder.Services.AddScoped<INameSearchService, NameSearchService>();
builder.Services.AddScoped<IMarriageLicenseService, MarriageLicenseSearchService>();
builder.Services.AddScoped<IDocumentFileService, DocumentFileService>();

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

// Use the global exception handling middleware
app.UseMiddleware<GlobalExceptionMiddleware>();

// CheckLive endpoint
app.MapGet("/checklive", (ICheckLiveService checkLiveService) =>
{
    return Results.Ok(new { live = checkLiveService.IsLive() });
})
.WithName("CheckLive")
.WithOpenApi();

// Notebooks endpoint
app.MapGet("/notebooks", async (
    [FromServices] INotebookService notebookService,
    [FromServices] IMapper mapper,
    [FromQuery] int? count) =>
{
    var notebooks = await notebookService.GetAllNotebooksAsync(count ?? 500);
    var notebooksDto = mapper.Map<IEnumerable<NotebookDto>>(notebooks);
    return Results.Ok(notebooksDto);
})
.WithName("GetAllNotebooks")
.WithMetadata(new HttpMethodMetadata(new[] { "GET" }));

// LandSearch endpoint
app.MapGet("/search/documents/book-page", async (
    [FromServices] ILandSearchPageBookService searchService,
    [FromServices] IMapper mapper,
    [FromQuery] long? book,
    [FromQuery] long? page) =>
{
    // Check if page is null and return 400 Bad Request if it is

    var notebooks = await searchService.SearchByPageBookService(book ?? 0, page ?? 0);
    var notebooksDto = mapper.Map<IEnumerable<NotebookDto>>(notebooks);
    return Results.Ok(notebooksDto);
})
.WithName("GetDocumentsByBookPage")
.WithMetadata(new HttpMethodMetadata(new[] { "GET" }));

// KindSearch endpoint
app.MapGet("/search/documents/kind", async (
    HttpContext context,
    [FromServices] IKindSearchService searchService,
    [FromServices] IMapper mapper) =>
{
    var kindsQuery = context.Request.Query["kinds"].ToString();
    var kinds = kindsQuery.Split(',').Select(k => k.Trim()).ToList();

    var notebooks = await searchService.SearchByKindsAsync(kinds);
    var notebooksDto = mapper.Map<IEnumerable<NotebookDto>>(notebooks);
    return Results.Ok(notebooksDto);
})
.WithName("GetDocumentsByKind")
.WithMetadata(new HttpMethodMetadata(new[] { "GET" }));

//NameSearch endpoint
app.MapGet("/search/documents/name", async (
    HttpContext context,
    [FromServices] INameSearchService searchService,
    [FromServices] IMapper mapper,
    [FromQuery] string surname,
    [FromQuery] string nameType,
    [FromQuery] string? given) =>
{
    var notebooks = await searchService.SearchByNameServiceAsync(surname, nameType ?? "BOTH", given);
    var notebooksDto = mapper.Map<IEnumerable<NotebookDto>>(notebooks);
    return Results.Ok(notebooksDto);
})
.WithName("GetDocumentsByName")
.WithMetadata(new HttpMethodMetadata(new[] { "GET" }));

//MarriageLicenseSearch endpoint
app.MapGet("/search/documents/marriage-license", async (
    HttpContext context,
    [FromServices] IMarriageLicenseService searchService,
    [FromServices] IMapper mapper,
    [FromQuery] string surname,
    [FromQuery] string? searchType,
    [FromQuery] int? order) =>
{
    var licenses = await searchService.SearchMarriageLicense(surname, searchType ?? "GROOM", order ?? 0);
    var licensesDto = mapper.Map<IEnumerable<MarriageLicenseDto>>(licenses);
    return Results.Ok(licensesDto);
})
.WithName("GetMarriageLicenses")
.WithMetadata(new HttpMethodMetadata(new[] { "GET" }));

// Endpoints for PDF and TIFF documents
app.MapGet("/search/documents/pdf", async (
    [FromServices] IDocumentFileService documentService,
    [FromQuery] string book,
    [FromQuery] string page) =>
{
   var fileContent = await documentService.GetDocumentFileAsync(book, page, "pdf");
   return Results.File(fileContent, "application/pdf", $"Book{book}_Page{page}.pdf"); 
})
.WithName("GetPdfDocument")
.WithMetadata(new HttpMethodMetadata(new[] { "GET" }));

app.MapGet("/search/documents/tif", async (
    [FromServices] IDocumentFileService documentService,
    [FromQuery] string book,
    [FromQuery] string page) =>
{
    var fileContent = await documentService.GetDocumentFileAsync(book, page, "tif");
    return Results.File(fileContent, "image/tiff", $"Book{book}_Page{page}.tif");   
})
.WithName("GetTifDocument")
.WithMetadata(new HttpMethodMetadata(new[] { "GET" }));

app.Run();
