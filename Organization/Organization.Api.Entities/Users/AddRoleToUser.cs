using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Organization.Api.Common;
using Organization.Api.Infrastructure.Persistance;

namespace Organization.Api.Features.Users
{
    public class AddRoleToUserController() : ApiControllerBase
    {

        [HttpPost("/api/users/{id}/roles")]
        public async Task<IResult> GetUsers(
            [FromRoute] Guid id,
            [FromForm] string name
        )
        {
            AddRoleToUserQuery query = new AddRoleToUserQuery(id, name);
            var result = await Mediator.Send(query);

            return result.Match(
                id => Results.NoContent(),
                error => Results.BadRequest(error.First().Description));
        }
    }

    public record AddRoleToUserQuery(Guid Id, string Name) : IRequest<ErrorOr<Guid>>;

    internal sealed class AddRoleToUserQueryValidator : AbstractValidator<AddRoleToUserQuery>
    {
        private readonly UserDatabaseContext _context;

        public AddRoleToUserQueryValidator(UserDatabaseContext context)
        {
            _context = context;

        }
    }

    internal static class AddRoleToUserQueryValidatorConstants
    {

    }

    public sealed class AddRoleToUserQueryHandler(UserDatabaseContext _context) : IRequestHandler<AddRoleToUserQuery, ErrorOr<Guid>>
    {
        public async Task<ErrorOr<Guid>> Handle(AddRoleToUserQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var role = _context.Roles.SingleOrDefault(x => x.Name == request.Name);
                if (role is null)
                    return Error.NotFound("The role was not found.");

                var user = _context.Users.Where(u => u.Id == request.Id)
                    .Include(u => u.Roles)
                    .SingleOrDefault();
                if (user is null)
                    return Error.NotFound("The user is not found.");

                user.AddRole(role);
                await _context.SaveChangesAsync();

                return user.Id;
            }
            catch (Exception ex)
            {
                return Error.Unexpected(ex.Message);
            }
        }
    }
}
