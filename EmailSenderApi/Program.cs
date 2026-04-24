using EmailSenderApi.Services;
using EmailSenderApi.Settings;
using Hangfire;
using Hangfire.InMemory;

var builder = WebApplication.CreateBuilder(args);

// SMTP
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
builder.Services.AddScoped<IEmailService, EmailService>();

// Templates
builder.Services.AddSingleton<ITemplateService, TemplateService>();

// Fila com Hangfire (InMemory — troque por SQL Server/SQLite em produção)
builder.Services.AddHangfire(config =>
    config.UseInMemoryStorage(new InMemoryStorageOptions
    {
        MaxExpirationTime = TimeSpan.FromDays(7)
    }));
builder.Services.AddHangfireServer();
builder.Services.AddScoped<IEmailQueueService, EmailQueueService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Email Sender API", Version = "v1" });
    c.EnableAnnotations();
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// Dashboard do Hangfire — acesse em /hangfire
app.UseHangfireDashboard("/hangfire");

app.UseAuthorization();
app.MapControllers();

app.Run();
