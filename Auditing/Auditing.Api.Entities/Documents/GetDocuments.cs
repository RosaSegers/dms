using Auditing.Api.Common;
using Auditing.Api.Common.Constants;
using Auditing.Api.Common.Interfaces;
using Auditing.Api.Common.Models;
using Auditing.Api.Domain.Events;
using Auditing.Api.Infrastructure.Persistance;
using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace Auditing.Api.Features.Auditings
{
    public class GetAuditingsController() : ApiControllerBase
    {
        [HttpGet("/api/Auditings")]
        public async Task<IResult> GetAuditingsUsingPagination([FromQuery] GetAuditingsWithPaginationQuery query)
        {
            var result = await Mediator.Send(query);

            return result.Match(
                id => Results.Ok(result.Value),
                error => Results.BadRequest(error.First().Description));
        }
    }

    public record GetAuditingsWithPaginationQuery(int PageNumber = 1, int PageSize = 10, bool IsDeleted = false) : IRequest<ErrorOr<PaginatedList<Domain.Entities.Auditing>>>;

    internal sealed class GetAuditingsWithPaginationQueryValidator : AbstractValidator<GetAuditingsWithPaginationQuery>
    {
        public GetAuditingsWithPaginationQueryValidator()
        {
            RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1)
                .WithMessage(GetAuditingsWithPaginationQueryConstants.PAGENUMBER_GREATER_THAN_STRING);

            RuleFor(x => x.PageSize).GreaterThanOrEqualTo(1)
                .WithMessage(GetAuditingsWithPaginationQueryConstants.PAGESIZE_GREATER_THAN_STRING);
        }
    }

    internal static class GetAuditingsWithPaginationQueryConstants
    {
        internal static string PAGENUMBER_GREATER_THAN_STRING = "PageNumber at least greater than or equal to 1.";
        internal static string PAGESIZE_GREATER_THAN_STRING = "PageSize at least greater than or equal to 1.";
    }


    public sealed class GetAuditingsWithPaginationQueryHandler(IAuditingStorage storage, ICacheService cache) : IRequestHandler<GetAuditingsWithPaginationQuery, ErrorOr<PaginatedList<Domain.Entities.Auditing>>>
    {
        private readonly IAuditingStorage _storage = storage;
        private readonly ICacheService _cache = cache;

        public async Task<ErrorOr<PaginatedList<Domain.Entities.Auditing>>> Handle(GetAuditingsWithPaginationQuery request, CancellationToken cancellationToken)
        {
            var cacheKey = CacheKeys.GetAuditingsCacheKey(request.PageNumber, request.PageSize, request.IsDeleted);
            if (_cache.TryGetCache(cacheKey, out object cachedAuditings))
            {
                return (PaginatedList<Domain.Entities.Auditing>?)cachedAuditings??new PaginatedList<Domain.Entities.Auditing>(new List<Domain.Entities.Auditing>(), 0, request.PageNumber, request.PageSize);
            }

            var Auditings = new List<Domain.Entities.Auditing>();
            var events = (await _storage.GetAuditingList()).GroupBy(e => e.Id).ToList();

            foreach (var group in events) 
            {
                if (group.Any(x => x.GetType() == typeof(AuditingDeletedEvent)) && !request.IsDeleted)
                    continue;

                var doc = new Domain.Entities.Auditing();
                foreach (var e in group.OrderBy(e => e.OccurredAt))
                    doc.Apply(e);

                Auditings.Add(doc);
            }

            var paginatedAuditings = Auditings
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();
            var paginatedQuery = new PaginatedList<Domain.Entities.Auditing>(paginatedAuditings, Auditings.Count, request.PageNumber, request.PageSize);

            _cache.SetCache(cacheKey, paginatedQuery);

            return paginatedQuery;
        }
    }
}
