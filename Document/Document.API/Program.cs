using Document.Api.Common.Interfaces;
using Document.Api.Features;
using Document.Api.Infrastructure;
using Document.Api.Infrastructure.Services;
using Prometheus;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Configuration.AddJsonFile("/Secrets/document-secrets.json", optional: false, reloadOnChange: false);

        builder.Services.AddCors(options =>
        {
            options.AddPolicy(
                name: "ApiGateway",
                policy =>
                {
                    policy.WithOrigins("api-gateway")
                                        .AllowAnyHeader()
                                        .AllowAnyMethod();
                });
        });

        builder.Services.AddControllers();
        builder.Services.AddHttpContextAccessor();

        builder.Services.AddHttpClient<IVirusScanner, VirusScanner>();

        builder.Services.AddApplication(builder.Configuration);
        builder.Services.AddInfrastructure();
        builder.Services.AddHealthChecks();

        var app = builder.Build();
        app.UseHttpsRedirection();
        app.UseRouting();

        app.UseHttpMetrics();

        app.UseCors("ApiGateway");
        app.MapHealthChecks("/health");
        app.MapHealthChecks("/ready");

        app.MapMetrics();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}