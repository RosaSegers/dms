using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using User.API.Common.Models;
using User.API.Common;
using MediatR;
using FluentValidation;
using User.Api.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;
using User.API.Common.Constants;
using User.Api.Common.Interfaces;
using ErrorOr;
using Microsoft.AspNetCore.Http;
using User.Api.Common.Authorization.Requirements;
using User.API.Common.Interfaces;

namespace User.Api.Features.Users
{
    public class DeleteUserController() : ApiControllerBase
    {

        [HttpDelete("/api/users/")]
        public async Task<IResult> GetUsers([FromRoute] DeleteUserCommand command)
        {
            var result = await Mediator.Send(command);

            return result.Match(
                id => Results.Ok(id),
                error => Results.BadRequest(error.First().Code));
        }
    }

    public record DeleteUserCommand() : IRequest<ErrorOr<Unit>>;

    public sealed class DeleteUserCommandHandler(UserDatabaseContext context, ICurrentUserService userService) : IRequestHandler<DeleteUserCommand, ErrorOr<Unit>>
    {
        private readonly UserDatabaseContext _context = context;


        public async Task<ErrorOr<Unit>> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (userService.UserId == null || userService.UserId == Guid.Empty)
                    return Error.Validation("id", "A valid user ID is required.");

                var user = await _context.Users
                    .SingleOrDefaultAsync(x => x.Id == userService.UserId, cancellationToken);

                if (user is null)
                    return Error.NotFound("User not found.");

                _context.Users.Remove(user);

                await _context.SaveChangesAsync(cancellationToken);

                return Unit.Value;
            }
            catch (DbUpdateConcurrencyException)
            {
                return Error.Conflict("The user was modified or deleted by another process.");
            }
            catch (Exception)
            {
                return Error.Unexpected("An unexpected error occurred while deleting the user.");
            }
        }
    }
}
