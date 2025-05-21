using AccessControl.Api.Common;
using AccessControl.Api.Infrastructure.Persistance;
using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AccessControl.Api.Features.Grants
{
    [ApiController]
    public class DeleteGrantController() : ApiControllerBase
    {
        [HttpDelete("/api/grants")]
        public async Task<IResult> Revoke([FromBody] DeleteGrantCommand command)
        {
            var result = await Mediator.Send(command);

            return result.Match(
                _ => Results.NoContent(),
                error => Results.BadRequest(error.First().Description));
        }

        public record DeleteGrantCommand(Guid UserId, Guid ResourceId, string Permission)
            : IRequest<ErrorOr<Deleted>>;

        internal sealed class DeleteGrantValidator : AbstractValidator<DeleteGrantCommand>
        {
            public DeleteGrantValidator()
            {
                RuleFor(x => x.UserId).NotEmpty();
                RuleFor(x => x.ResourceId).NotEmpty();
                RuleFor(x => x.Permission).NotEmpty();
            }
        }

        public sealed class DeleteGrantHandler(Context context)
            : IRequestHandler<DeleteGrantCommand, ErrorOr<Deleted>>
        {
            public async Task<ErrorOr<Deleted>> Handle(DeleteGrantCommand request, CancellationToken cancellationToken)
            {
                var grant = await context.Grants
                    .SingleOrDefaultAsync(x => x.UserId == request.UserId &&
                                              x.ResourceId == request.ResourceId &&
                                              x.Permission.Name == request.Permission, cancellationToken);

                if (grant is null)
                    return Error.NotFound("Grant.NotFound", "The specified permission grant does not exist.");

                context.Grants.Remove(grant);
                await context.SaveChangesAsync(cancellationToken);

                return Result.Deleted;
            }
        }
    }
}