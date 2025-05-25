using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace User.API.Common
{
    [ApiController]
    public class ApiControllerBase : ControllerBase
    {
        private ISender? _mediator;

        protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetService<ISender>() ?? throw new Exception();

        protected ActionResult Problem(List<Error> errors)
        {
            if (errors.All(error => error.Type == ErrorType.Validation))
                return ValidationProblem(errors);

            throw new NotImplementedException();

        }

        private ActionResult ValidationProblem(List<Error> errors)
        {
            var modelStateDictionary = new ModelStateDictionary();

            errors.ForEach(error => modelStateDictionary.AddModelError(error.Code, error.Description));

            return ValidationProblem(modelStateDictionary);
        }

    }
}
