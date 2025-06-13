using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Document.Api.Infrastructure.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Document.Api.Features.Documents
{
    public class DocumentController : ControllerBase
    {
        [HttpGet("/api/documents/{id}/status")]
        public async Task<IResult> GetStatus([FromServices] IDocumentStatusService statusService, Guid id)
        {
            var status = await statusService.GetStatusAsync(id);
            return status == "not_found"
                ? Results.NotFound($"Document with ID {id} not found.")
                : Results.Ok(new { DocumentId = id, Status = status });
        }
    }
}
