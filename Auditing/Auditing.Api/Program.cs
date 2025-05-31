using Auditing.Api.Features;
using Auditing.Api.Infrastructure;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddCors(options =>
        {
            options.AddPolicy(name: "ApiGateway",
                policy =>
                {
                    policy.WithOrigins(builder.Configuration["Gateway"] ?? "")
                                        .AllowAnyHeader()
                                        .AllowAnyMethod();
                });
        });

        builder.Services.AddControllers();
        builder.Services.AddHttpContextAccessor();

        builder.Services.AddApplication();
        builder.Services.AddInfrastructure(builder.Configuration);
        builder.Services.AddHealthChecks();

        builder.WebHost.UseUrls("http://localhost:80");

        var app = builder.Build();
        app.UseHttpsRedirection();
        app.UseRouting();

        app.UseCors("ApiGateway");  
        app.MapHealthChecks("/health");

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
