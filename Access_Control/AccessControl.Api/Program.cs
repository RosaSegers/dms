using AccessControl.Api.Features;
using AccessControl.Api.Domain;
using AccessControl.Api.Infrastructure;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Configuration.AddJsonFile("/Secrets/accesscontrol-secrets.json", optional: false, reloadOnChange: false);

        builder.Services.AddCors(options =>
        {
            options.AddPolicy(name: "ApiGateway",
                policy =>
                {
                    policy.WithOrigins(builder.Configuration["Gateway"] ?? throw new Exception())
                                        .AllowAnyHeader()
                                        .AllowAnyMethod();
                });
        });

        builder.Services.AddControllers();
        builder.Services.AddHttpContextAccessor();

        builder.Services.AddApplication(builder.Configuration);
        builder.Services.AddInfrastructure();
        builder.Services.AddValidation();
        builder.Services.AddMapping();


        var app = builder.Build();
        app.UseHttpsRedirection();
        app.UseRouting();

        app.UseCors("ApiGateway");  

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
