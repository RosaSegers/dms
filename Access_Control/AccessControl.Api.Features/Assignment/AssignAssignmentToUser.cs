using AccessControl.Api.Common;
using AccessControl.Api.Infrastructure.Persistance;
using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AccessControl.Api.Features.Assignment
{
    public class AssignmentsController : ApiControllerBase
    {
        [HttpPost("/api/assignments")]
        public async Task<IResult> AssignAssignmentToUser([FromBody] AssignAssignmentToUserCommand command)
        {
            var result = await Mediator.Send(command);

            return result.Match(
                _ => Results.Created(),
                error => Results.BadRequest(error.First().Description)
            );
        }
    }

    public record AssignAssignmentToUserCommand(Guid UserId, Guid ResourceId, Guid RoleId) : IRequest<ErrorOr<Unit>>;

    internal sealed class AssignAssignmentToUserCommandValidator : AbstractValidator<AssignAssignmentToUserCommand>
    {
        public AssignAssignmentToUserCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required.");

            RuleFor(x => x.ResourceId)
                .NotEmpty().WithMessage("Resource ID is required.");

            RuleFor(x => x.RoleId)
                .NotEmpty().WithMessage("Permission is required.");
        }
    }

    public sealed class AssignAssignmentToUserCommandHandler(Context context) : IRequestHandler<AssignAssignmentToUserCommand, ErrorOr<Unit>>
    {
        private readonly Context _context = context;

        public async Task<ErrorOr<Unit>> Handle(AssignAssignmentToUserCommand request, CancellationToken cancellationToken)
        {
            
            var assignment = new Domain.Entities.Assignment(request.UserId, request.ResourceId, _context.Roles.Single(x => x.Id == request.RoleId));

            await _context.Assignment.AddAsync(assignment);
            await _context.SaveChangesAsync(); 

            return Unit.Value;
        }
    }
}
