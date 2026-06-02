using AirlineReservation.Api.Infrastructure;

const string LocalDevelopmentCorsPolicy = "LocalDevelopmentCors";

var builder = WebApplication.CreateBuilder(args);

builder.AddAirlineLogging();
builder.Services.AddAirlineDatabase(builder.Configuration);
builder.Services.AddAirlineServices();
builder.Services.AddAirlineJwt(builder.Configuration);
builder.Services.AddAirlineSwagger();
builder.Services.AddCors(options =>
{
    options.AddPolicy(LocalDevelopmentCorsPolicy, policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5000",
                "https://localhost:7150",
                "http://localhost:5235",
                "https://localhost:7186")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
builder.Services.AddControllers();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
await DatabaseSeeder.SeedAsync(app.Services);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors(LocalDevelopmentCorsPolicy);
}
else
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
