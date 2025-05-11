using Auditing.Api.Common;
using Auditing.Api.Common.Interfaces;
using Auditing.Api.Domain.Events;
using Auditing.Api.Infrastructure.Persistance;
using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auditing.Api.Features.Auditings
{
    public class UpdateAuditingsController() : ApiControllerBase
    {
        [HttpPut("/api/Auditings/{id:guid}")]
        public async Task<IResult> UploadAuditing(
            [FromRoute] Guid id,
            [FromForm] string name,
            [FromForm] string description,
            [FromForm] float version,
            [FromForm] IFormFile file)
        {
            var query = new UpdateAuditingQuery(id, name, description, version, file);
            var result = await Mediator.Send(query);

            return result.Match(
                id => Results.NoContent(),
                error => Results.BadRequest(error.First().Description));
        }
    }

    public record UpdateAuditingQuery(Guid Id, string Name, string Description, float Version, IFormFile File) : IRequest<ErrorOr<Guid>>;

    internal sealed class UpdateAuditingQueryValidator : AbstractValidator<UpdateAuditingQuery>
    {
        private readonly IVirusScanner _scanner;
        private readonly IAuditingStorage _storage;

        public UpdateAuditingQueryValidator(IVirusScanner scanner, IAuditingStorage storage)
        {
            _scanner = scanner;
            _storage = storage;

            RuleFor(x => x.Id)
                .MustAsync(NotBeDeleted).WithMessage(UpdateAuditingQueryValidatorConstants.FILE_DELETED);

            RuleFor(x => x.File)
                .MustAsync(NotBeVirus).WithMessage(UpdateAuditingQueryValidatorConstants.MALICIOUS_FILE);
        }

        private async Task<bool> NotBeVirus(IFormFile file, CancellationToken token) => (await _scanner.ScanFile(file));
        private async Task<bool> NotBeDeleted(Guid id, CancellationToken token)
            => !(await _storage.GetAuditingById(id)).Any(x => x.GetType() == typeof(AuditingDeletedEvent));
    } 

    internal static class UpdateAuditingQueryValidatorConstants
    {
        internal static string FILE_DELETED = "Sorry, the file has previously been deleted so it can't be edited";
        internal static string MALICIOUS_FILE = "Please don't upload malicious files";
    }


    public sealed class UpdateAuditingQueryHandler(IAuditingStorage storage, ICurrentUserService userService) : IRequestHandler<UpdateAuditingQuery, ErrorOr<Guid>>
    {
        private readonly IAuditingStorage _storage = storage;
        private readonly ICurrentUserService _userService = userService;

        public async Task<ErrorOr<Guid>> Handle(UpdateAuditingQuery request, CancellationToken cancellationToken)
        {
            var e = new AuditingUpdatedEvent(request.Id, request.Name, request.Description, request.Version, request.File, "", _userService.UserId);

            if (await _storage.AddAuditing(e))
                return e.Id;
            return Error.Failure("something went wrong trying so save the file.");
        }
    }
}
