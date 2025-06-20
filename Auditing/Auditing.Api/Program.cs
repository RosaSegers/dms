using Auditing.Api.Features;
using Auditing.Api.Infrastructure;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Configuration.AddJsonFile("/Secrets/auditing-secrets.json", optional: false, reloadOnChange: false);

        builder.Services.AddCors(options =>
        {
            options.AddPolicy(name: "ApiGateway",
                policy =>
                {
                    policy.WithOrigins("api-gateway")
                                        .AllowAnyHeader()
                                        .AllowAnyMethod();
                });
        });

        builder.Services.AddControllers();
        builder.Services.AddHttpContextAccessor();

        builder.Services.AddApplication(builder.Configuration);
        builder.Services.AddInfrastructure(builder.Configuration);
        builder.Services.AddHealthChecks();

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
