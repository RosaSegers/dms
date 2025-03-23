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

namespace User.Api.Features.Users
{
    public class CreateUsersController() : ApiControllerBase
    {
        [HttpPost("/api/users")]
        public async Task<Guid> GetUsers([FromBody] CreateUserQuery query)
        {
            var result = await Mediator.Send(query);

            return result;
        }
    }

    public record CreateUserQuery(string username, string email, string password) : IRequest<Guid>;

    internal sealed class CreateUserQueryValidator : AbstractValidator<CreateUserQuery>
    {
        private UserDatabaseContext _context;

        public CreateUserQueryValidator(UserDatabaseContext context)
        {
            _context = context;

            RuleFor(user => user.username)
                .MustAsync(BeUniqueUsername).WithMessage("Test");

            RuleFor(user => user.email)
                .MustAsync(BeUniqueEmail).WithMessage("Test");
        }

        private Task<bool> BeUniqueUsername(string username, CancellationToken token) => _context.Users.AllAsync(x => x.Name != username);

        private Task<bool> BeUniqueEmail(string email, CancellationToken token) => _context.Users.AllAsync(x => x.Email != email);
    }

    internal static class CreateUserQueryValidatorConstants
    {

    }

    public sealed class CreateUserQueryHandler(UserDatabaseContext context) : IRequestHandler<CreateUserQuery, Guid>
    {
        private readonly UserDatabaseContext _context = context;


        public async Task<Guid> Handle(CreateUserQuery request, CancellationToken cancellationToken)
        {
            var user = new Domain.Entities.User(request.username, request.email, request.password);
            var x = await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return user.Id;
        }
    }
}
