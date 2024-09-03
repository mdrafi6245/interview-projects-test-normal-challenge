using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using SampleAPI.Entities;
using SampleAPI.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddDbContext<SampleApiDbContext>(options => options.UseInMemoryDatabase(databaseName: "SampleDB"));
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// This logs the details to the console * We can use Serilogs to log to files and AWS CloudWatch to log it to cloud, Sumo Logic Logs, New Relic logs, database etc.,
builder.Services.AddLogging();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


// Global Exception Handling Middleware
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        var exception = exceptionHandlerPathFeature?.Error;

        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(exception, "An unexpected error occurred."); // Log the exception details

        var errorResponse = new
        {
            Message = "An unexpected error occurred. Please try again later.",
            // Optionally include more information, like a correlation ID for tracking.
        };

        await context.Response.WriteAsJsonAsync(errorResponse); // Send a standardized error response to the client
    });
});


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

