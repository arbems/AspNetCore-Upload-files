using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Configure<FormOptions>(options =>
{
    // Set the limit to 256 MB, by default 128 MB
    options.MultipartBodyLengthLimit = 256 * 1024 * 1024;
    // Set the limit to 1 MB between small and large files
    options.MemoryBufferThreshold = 1 * 1024 * 1024;
});

// Handle requests up to 256 MB
builder.WebHost.ConfigureKestrel(ops => ops.Limits.MaxRequestBodySize = 256 * 1024 * 1024);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
