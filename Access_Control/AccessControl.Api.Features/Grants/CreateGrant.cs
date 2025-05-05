using AccessControl.Api.Common;
using AccessControl.Api.Infrastructure.Persistance;
using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AccessControl.Api.Features.Grants
{
    [ApiController]
    public class CreateGrantController() : ApiControllerBase
    {
        [HttpPost("/api/grants")]
        public async Task<IResult> Grant([FromBody] CreateGrantCommand command)
        {
            var result = await Mediator.Send(command);

            return result.Match(
                _ => Results.Created(),
                error => Results.BadRequest(error.First().Description));
        }

        public record CreateGrantCommand(Guid UserId, Guid ResourceId, string Permission)
            : IRequest<ErrorOr<Unit>>;

        internal sealed class CreateGrantValidator : AbstractValidator<CreateGrantCommand>
        {
            public CreateGrantValidator()
            {
                RuleFor(x => x.UserId).NotEmpty();
                RuleFor(x => x.ResourceId).NotEmpty();
                RuleFor(x => x.Permission).NotEmpty().MaximumLength(100);
            }
        }

        public sealed class CreateGrantHandler(Context context)
            : IRequestHandler<CreateGrantCommand, ErrorOr<Unit>>
        {
            private readonly Context _context = context;

            public async Task<ErrorOr<Unit>> Handle(CreateGrantCommand request, CancellationToken cancellationToken)
            {
                var grant = new Domain.Entities.Grant(request.UserId, request.ResourceId, request.Permission);

                _context.Grants.Add(grant);
                await _context.SaveChangesAsync(cancellationToken);

                return Unit.Value;
            }
        }
    }
}