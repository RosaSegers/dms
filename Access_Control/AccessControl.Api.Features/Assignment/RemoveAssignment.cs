using AccessControl.Api.Common;
using AccessControl.Api.Infrastructure.Persistance;
using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AccessControl.Api.Features.Assignment
{
    public class RemoveAssignmentController : ApiControllerBase
    {
        [HttpDelete("/api/assignments")]
        public async Task<IResult> RemoveAssignment([FromBody] RemoveAssignmentCommand command)
        {
            var result = await Mediator.Send(command);

            return result.Match(
                _ => Results.NoContent(),
                error => Results.BadRequest(error.First().Description)
            );
        }
    }

    public record RemoveAssignmentCommand(Guid UserId, Guid ResourceId, Guid RoleId) : IRequest<ErrorOr<Unit>>;

    internal sealed class RemoveAssignmentCommandValidator : AbstractValidator<RemoveAssignmentCommand>
    {
        public RemoveAssignmentCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required.");

            RuleFor(x => x.ResourceId)
                .NotEmpty().WithMessage("Resource ID is required.");

            RuleFor(x => x.RoleId)
                .NotEmpty().WithMessage("Role ID is required.");


        }
    }

    public sealed class RemoveRoleAssignmentCommandHandler(Context context) : IRequestHandler<RemoveAssignmentCommand, ErrorOr<Unit>>
    {
        private readonly Context _context = context;

        public async Task<ErrorOr<Unit>> Handle(RemoveAssignmentCommand request, CancellationToken cancellationToken)
        {
            var assignment = await _context.Assignment
                .FirstOrDefaultAsync(a => a.UserId == request.UserId
                                       && a.ResourceId == request.ResourceId
                                       && a.RoleId == request.RoleId, cancellationToken);

            if (assignment == null)
            {
                return Error.NotFound("Role assignment not found.");
            }

            _context.Assignment.Remove(assignment);
            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
