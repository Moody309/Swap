using Swap.API;
using Swap.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddApiServices()
       .AddObservability()
       .AddDatabase();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    await app.ApplyMigrationsAsync();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

await app.RunAsync();
