using Document.Api.Common.Behaviour;
using Document.Api.Infrastructure.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Document.Api.Features
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            //services.AddScoped<RabbitMqLogProducer>();

            services.AddMediatR(options =>
            {
                options.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
                options.AddOpenBehavior(typeof(ValidationBehaviour<,>));
                options.AddOpenBehavior(typeof(LoggingBehaviour<,>));
            });

            services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, includeInternalTypes: true);

            return services;
        }
    }
}
