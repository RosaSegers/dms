using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Consul;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Configuration.AddJsonFile("configuration.json", optional: false, reloadOnChange: true);
builder.Services.AddOcelot(builder.Configuration)
    .AddConsul();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();
await app.UseOcelot();
app.UseAuthorization();
app.MapControllers();

app.Run();
