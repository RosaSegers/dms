using Auditing.Api.Common;
using Auditing.Api.Common.Constants;
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
    public class GetAuditingByIdController() : ApiControllerBase
    {
        [HttpGet("/api/Auditings/{Id}")]
        public async Task<IResult> GetAuditingsUsingPagination([FromRoute] GetAuditingByIdQuery query)
        {
            var result = await Mediator.Send(query);

            return result.Match(
                id => Results.Ok(result.Value),
                error => Results.BadRequest(error.First().Description));
        }
    }

    public record GetAuditingByIdQuery(Guid Id) : IRequest<ErrorOr<Domain.Entities.Auditing>>;

    internal sealed class GetAuditingByIdQueryValidator : AbstractValidator<GetAuditingByIdQuery>
    {
        public GetAuditingByIdQueryValidator()
        {

        }
    }

    internal static class GetAuditingByIdQueryConstants
    {

    }


    public sealed class GetAuditingByIdQueryHandler(IAuditingStorage storage, ICacheService cache) : IRequestHandler<GetAuditingByIdQuery, ErrorOr<Domain.Entities.Auditing>>
    {
        private readonly IAuditingStorage _storage = storage;
        private readonly ICacheService _cache = cache;

        public async Task<ErrorOr<Domain.Entities.Auditing>> Handle(GetAuditingByIdQuery request, CancellationToken cancellationToken)
        {
            var cacheKey = CacheKeys.GetAuditingCacheKey(request.Id);
            if (_cache.TryGetCache(cacheKey, out object cachedAuditings))
            {
                return (Domain.Entities.Auditing?)cachedAuditings;
            }

            var events = (await _storage.GetAuditingList()).Where(x => x.Id == request.Id);

            var doc = new Domain.Entities.Auditing();
            foreach (var e in events.OrderBy(e => e.OccurredAt))
                doc.Apply(e);

            _cache.SetCache(cacheKey, doc);

            return doc;
        }
    }
}
