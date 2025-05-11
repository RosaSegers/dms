using Auditing.Api.Common;
using Auditing.Api.Common.Interfaces;
using Auditing.Api.Domain.Events;
using Auditing.Api.Infrastructure.Persistance;
using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Auditing.Api.Features.Auditings
{
    public class UploadAuditingsController() : ApiControllerBase
    {
        [HttpPost("/api/Auditings")]
        public async Task<IResult> UploadAuditing([FromForm] UploadAuditingQuery query)
        {
            var result = await Mediator.Send(query);

            return result.Match(
                id => Results.Created("/api/Auditings", id),
                error => Results.BadRequest(error.First().Description));
        }
    }

    public record UploadAuditingQuery(string Name, string Description, float Version, IFormFile File) : IRequest<ErrorOr<Guid>>;

    internal sealed class UploadAuditingQueryValidator : AbstractValidator<UploadAuditingQuery>
    {
        private readonly IVirusScanner _scanner;
        private readonly IAuditingStorage _storage;

        public UploadAuditingQueryValidator(IVirusScanner scanner, IAuditingStorage storage)
        {
            _scanner = scanner;
            _storage = storage;

            RuleFor(x => x.File)
                .MustAsync(NotBeVirus).WithMessage(UploadAuditingQueryValidatorConstants.MALICIOUS_FILE);
        }

        private async Task<bool> NotBeVirus(IFormFile file, CancellationToken token) => (await _scanner.ScanFile(file));
    }

    internal static class UploadAuditingQueryValidatorConstants
    {
        internal static string MALICIOUS_FILE = "Please don't upload malicious files";
    }


    public sealed class UploadAuditingQueryHandler(IAuditingStorage storage, ICurrentUserService userService) : IRequestHandler<UploadAuditingQuery, ErrorOr<Guid>>
    {
        private readonly IAuditingStorage _storage = storage;
        private readonly ICurrentUserService _userService = userService;

        public async Task<ErrorOr<Guid>> Handle(UploadAuditingQuery request, CancellationToken cancellationToken)
        {
            var e = new AuditingUploadedEvent(request.Name, request.Description, request.Version, request.File, "", _userService.UserId);

            if(await _storage.AddAuditing(e))
                return e.Id;
            return Error.Failure("something went wrong trying so save the file.");
        }
    }
}
