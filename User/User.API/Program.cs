using User.Api.Features;
using User.Api.Domain;
using User.Api.Infrastructure;

namespace User.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Configuration.AddJsonFile("/Secrets/user-secrets.json", optional: false, reloadOnChange: false);

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
            builder.Services.AddInfrastructure(builder.Configuration);
            builder.Services.AddValidation();
            builder.Services.AddMapping();
            builder.Services.AddHealthChecks();

            builder.WebHost.UseUrls("http://0.0.0.0:80");

            var app = builder.Build();
            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseCors("ApiGateway");
            app.MapHealthChecks("/health");

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}