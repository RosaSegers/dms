using Document.Api.Features;
using Document.Api.Infrastructure;

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
                    policy.WithOrigins("http://localhost:5285")
                                        .AllowAnyHeader()
                                        .AllowAnyMethod();
                });
        });

        builder.Services.AddControllers();
        builder.Services.AddHttpContextAccessor();

        builder.Services.AddApplication();
        builder.Services.AddInfrastructure();
        //builder.Services.AddValidation();


        var app = builder.Build();
        app.UseHttpsRedirection();
        app.UseRouting();

        app.UseCors("ApiGateway");  

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}