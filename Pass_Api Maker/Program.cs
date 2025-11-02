var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Enable serving static files from wwwroot folder
app.UseStaticFiles();

app.UseCors();
app.UseAuthorization();
app.MapControllers();

Console.WriteLine("API Server running on http://localhost:5135");
Console.WriteLine("Static files accessible at: http://localhost:5135/images/");

app.Run();