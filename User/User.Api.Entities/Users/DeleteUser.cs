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
                    return Error.Validation("id", "id is required");

                var user = await _context.Users
                    .SingleAsync(x => x.Id == userService.UserId, cancellationToken);
                _context.Users.Remove(user);

                await _context.SaveChangesAsync(cancellationToken);

                return Unit.Value;
            }
            catch (Exception ex)
            {
                return Error.Unexpected(ex.Message);
            }
        }
    }
}
