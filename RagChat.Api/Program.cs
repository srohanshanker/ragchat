using Microsoft.EntityFrameworkCore;
using RagChat.Api.Data;
using RagChat.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<OllamaOptions>(builder.Configuration.GetSection(OllamaOptions.SectionName));
builder.Services.Configure<RagOptions>(builder.Configuration.GetSection(RagOptions.SectionName));

var dbPath = Path.Combine(builder.Environment.ContentRootPath, "ragchat.db");
builder.Services.AddDbContext<RagDbContext>(options => options.UseSqlite($"Data Source={dbPath}"));

builder.Services.AddHttpClient<OllamaClient>();
builder.Services.AddScoped<IEmbeddingService>(sp => sp.GetRequiredService<OllamaClient>());
builder.Services.AddScoped<IChatCompletionService>(sp => sp.GetRequiredService<OllamaClient>());

builder.Services.AddSingleton<TextExtractionService>();
builder.Services.AddSingleton<TextChunker>();
builder.Services.AddScoped<VectorSearchService>();
builder.Services.AddScoped<DocumentIngestionService>();
builder.Services.AddScoped<RagChatService>();
builder.Services.AddScoped<SampleDocsSeeder>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<RagDbContext>();
    db.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseCors("AllowAll");
app.MapControllers();

app.Run();
