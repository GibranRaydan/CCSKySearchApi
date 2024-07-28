using Microsoft.OpenApi.Models;
using CCSWebKySearch.Models;
using CCSWebKySearch.Services;
using AutoMapper;
using Serilog;
using Microsoft.AspNetCore.Mvc;
using DotNetEnv;
using CCSWebKySearch.Dtos;
using CCSWebKySearch.Contracts;
using AspNetCoreRateLimit;
using Amazon.S3;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using Amazon.SecretsManager.Model;
using Amazon.SecretsManager;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Local Development: Load the .env file
if (builder.Environment.IsDevelopment())
{
    Env.Load();
}

// Production: Load secrets from AWS Secrets Manager
else
{
    var secretName = "prod/api/search/gibran";
    var region = "us-east-2";
    var client = new AmazonSecretsManagerClient(Amazon.RegionEndpoint.GetBySystemName(region));

    var request = new GetSecretValueRequest
    {
        SecretId = secretName,
        VersionStage = "AWSCURRENT", // VersionStage defaults to AWSCURRENT if unspecified.
    };

    var response = await client.GetSecretValueAsync(request);
    if (response.SecretString != null)
    {
        var secretValues = JsonSerializer.Deserialize<Dictionary<string, string>>(response.SecretString);
        foreach (var secret in secretValues)
        {
            builder.Configuration[secret.Key] = secret.Value;
        }
    }
}


// Retrieve values from .env or secret manager
var connectionString = Env.GetString("CONNECTION_STRING");
var documentsPath = Env.GetString("DOCUMENTS_PATH");
var seqServerUrl = Env.GetString("SEQ_SERVER_URL");
var rateLimitCheckLive = Env.GetString("RATE_LIMIT_CHECKLIVE");
var rateLimitSearch = Env.GetString("RATE_LIMIT_SEARCH");
var awsAccessKeyId = Env.GetString("AWS_ACCESS_KEY_ID");
var awsSecretAccessKey = Env.GetString("AWS_SECRET_ACCESS_KEY");
var awsRegion = Env.GetString("AWS_REGION");
var s3BucketName = Env.GetString("S3_BUCKET_NAME");

// Set up configuration to override values with those from .env
builder.Configuration.AddInMemoryCollection(new Dictionary<string, string>
{
    { "ConnectionStrings:DefaultConnection", connectionString },
    { "DocumentsPath", documentsPath },
    { "Serilog:WriteTo:2:Args:serverUrl", seqServerUrl },
    { "IpRateLimiting:GeneralRules:0:Limit", rateLimitCheckLive },
    { "IpRateLimiting:GeneralRules:1:Limit", rateLimitSearch },
    { "AWS:BucketName", s3BucketName }

});


builder.Services.AddAWSService<IAmazonS3>(new AWSOptions
{
    Credentials = new BasicAWSCredentials(awsAccessKeyId, awsSecretAccessKey),
    Region = Amazon.RegionEndpoint.GetBySystemName(awsRegion)
});

builder.Host.UseSerilog((ctx, lc) => lc.WriteTo.Console().ReadFrom.Configuration(ctx.Configuration));

// API services
builder.Services.AddScoped<ICheckLiveService, CheckLiveService>();
builder.Services.AddScoped<INotebookService, NotebookService>();
builder.Services.AddScoped<ILandSearchPageBookService, BookPageSearchService>();
builder.Services.AddScoped<IKindSearchService, KindSearchService>();
builder.Services.AddScoped<INameSearchService, NameSearchService>();
builder.Services.AddScoped<IMarriageLicenseService, MarriageLicenseSearchService>();
builder.Services.AddScoped<IDocumentFileService, DocumentFileService>();

// Add S3 service
builder.Services.AddScoped<S3DocumentService>();



// Add AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// CORS configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", b => b.AllowAnyHeader()
                                        .AllowAnyOrigin()
                                        .AllowAnyMethod());
});

// Add IpRateLimiting services
builder.Services.AddOptions();
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.Configure<IpRateLimitPolicies>(builder.Configuration.GetSection("IpRateLimitPolicies"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

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

// Enable IpRateLimiting middleware
app.UseIpRateLimiting();

// CheckLive endpoint
app.MapGet("/checklive", (ICheckLiveService checkLiveService) =>
{
    return Results.Ok(new { live = checkLiveService.IsLive() });
})
.WithName("CheckLive")
.WithOpenApi();

// Notebooks endpoint
app.MapGet("/search/documents/daily", async (
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

// NameSearch endpoint
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

// MarriageLicenseSearch endpoint
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
    [FromServices] S3DocumentService documentService,
    [FromQuery] string book,
    [FromQuery] string page) =>
{
    var fileContent = await documentService.GetMergedFileFromS3(book, page, "pdf");
    string folderPath = $"./tiff/BOOK{book}/";
    Directory.Delete(folderPath, true);
    return Results.File(fileContent, "application/pdf", $"Book{book}_Page{page}.pdf");
})
.WithName("GetPdfDocument")
.WithMetadata(new HttpMethodMetadata(new[] { "GET" }));

app.MapGet("/search/documents/tif", async (
    [FromServices] S3DocumentService documentService,
    [FromQuery] string book,
    [FromQuery] string page) =>
{
    var fileContent = await documentService.GetMergedFileFromS3(book, page, "tif");
    string folderPath = $"./tiff/BOOK{book}/";
    Directory.Delete(folderPath, true);
    return Results.File(fileContent, "image/tiff", $"Book{book}_Page{page}.tif");
})
.WithName("GetTifDocument")
.WithMetadata(new HttpMethodMetadata(new[] { "GET" }));

app.Run();
