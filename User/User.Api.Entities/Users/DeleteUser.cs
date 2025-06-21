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
using User.Api.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;

namespace User.Api.Features.Users
{
    public class DeleteUserController() : ApiControllerBase
    {
        [HttpDelete("/api/users/{Id:guid}")]
        public async Task<IResult> GetUsers([FromRoute] DeleteUserCommand command)
        {
            var result = await Mediator.Send(command);

            return result.Match(
                id => Results.Ok(id),
                error => Results.BadRequest(error.First().Code));
        }
    }

    public record DeleteUserCommand(Guid Id) : IRequest<ErrorOr<Unit>>;

    public sealed class DeleteUserCommandHandler(UserDatabaseContext context, ICurrentUserService userService, UserApiSaga userSaga) : IRequestHandler<DeleteUserCommand, ErrorOr<Unit>>
    {
        private readonly UserDatabaseContext _context = context;


        public async Task<ErrorOr<Unit>> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _context.Users
                    .SingleOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

                if (user is null)
                    return Error.NotFound("User not found.");

                await userSaga.StartDeleteSagaAsync(request.Id!);

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
