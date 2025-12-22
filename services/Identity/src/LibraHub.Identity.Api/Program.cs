using LibraHub.BuildingBlocks.Correlation;
using LibraHub.BuildingBlocks.Middlewares;
using LibraHub.BuildingBlocks.Observability;
using LibraHub.Identity.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddIdentitySwagger();
builder.Services.AddIdentityDatabase(builder.Configuration);
builder.Services.AddIdentityApplicationServices();
builder.Services.AddIdentityJwtAuthentication(builder.Configuration);
builder.Services.AddIdentityRabbitMq(builder.Configuration);

// Health checks
builder.Services.AddIdentityHealthChecks(builder.Configuration);

// Observability
builder.Services.AddTelemetry("LibraHub.Identity", "1.0.0");

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseIdentitySwagger();
}

app.UseHttpsRedirection();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Database migrations and seeding
app.UseIdentityDatabaseMigrations();
app.UseIdentityDatabaseSeeder();

app.Run();
