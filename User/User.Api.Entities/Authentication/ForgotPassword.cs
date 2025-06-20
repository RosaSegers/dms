using Azure;
using Azure.Communication.Email;
using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using User.Api.Domain.Entities;
using User.Api.Infrastructure.Persistance;
using User.API.Common;

namespace User.Api.Features.Authentication
{
    [AllowAnonymous]
    public class ForgotPasswordController() : ApiControllerBase
    {
        [HttpPost("/api/auth/ForgotPassword")]
        public async Task<IResult> ForgotPassword([FromForm] ForgotPasswordCommand query)
        {
            var result = await Mediator.Send(query);

            return result.Match(
                _ => Results.NoContent(),
                error => Results.BadRequest(error.First().Description));
        }
    }

    public record ForgotPasswordCommand(string Email) : IRequest<ErrorOr<Unit>>;

    internal sealed class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
    {
        public ForgotPasswordCommandValidator()
        {
            RuleFor(user => user.Email)
            .EmailAddress(FluentValidation.Validators.EmailValidationMode.AspNetCoreCompatible)
            .WithMessage(ForgotPasswordCommandConstants.EMAIL_INVALID_STRING)
            .Length(ForgotPasswordCommandConstants.EMAIL_MINIMUM_LENGTH, ForgotPasswordCommandConstants.EMAIL_MAXIMUM_LENGTH)
            .WithMessage(ForgotPasswordCommandConstants.EMAIL_INVALID_LENGTH_STRING);
        }
    }

    internal static class ForgotPasswordCommandConstants
    {
        public static string EMAIL_INVALID_STRING = "Supplied email is invalid";
        public static string EMAIL_INVALID_LENGTH_STRING = $"An email needs to be between {EMAIL_MINIMUM_LENGTH} and {EMAIL_MAXIMUM_LENGTH} characters.";

        public static int EMAIL_MINIMUM_LENGTH = 4;
        public static int EMAIL_MAXIMUM_LENGTH = 100;
    }

    public sealed class ForgotPasswordCommandHandler(UserDatabaseContext context) : IRequestHandler<ForgotPasswordCommand, ErrorOr<Unit>>
    {
        private readonly EmailClient emailClient = new("");

        public async Task<ErrorOr<Unit>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            var user = await context.Users.FirstAsync(x => x.Email.ToLower() == request.Email.ToLower());

            if (user is null)
                return Error.NotFound("User not found with the provided email.");

            var token = new PasswordResetToken() { UserId = user.Id };
            user.PasswordResetTokens.Add(token);

            await context.SaveChangesAsync(cancellationToken);

            return await SendEmailAsync(token, user);
        }

        private async Task<ErrorOr<Unit>> SendEmailAsync(PasswordResetToken token, Domain.Entities.User user)
        {
            var emailContent = new EmailContent("Forgot Password")
            {
                PlainText = $"Hello {user.Name},\n\n" +
                "You have requested a password reset. Please use the following link to reset your password:\n" +
                $"https://example.com/reset-password?token={token.Token}\n\n" +
                "If you did not request this, please ignore this email.\n\n" +
                "Best regards,\n" +
                "The Document Management Team",
            };
            var sender = "";
            var recipient = new EmailRecipients(new List<EmailAddress>{
                new EmailAddress(user.Email, user.Name)
            });

            var emailMessage = new EmailMessage(sender, recipient, emailContent);

            CancellationToken cancellationToken = new CancellationTokenSource(TimeSpan.FromMinutes(1)).Token;

            do
            {
                EmailSendOperation status = await emailClient.SendAsync(WaitUntil.Completed, emailMessage, cancellationToken);
                string messageId = status.Id;

                if (string.IsNullOrEmpty(messageId))
                    return Error.Unexpected("Failed to send email. Message ID is null or empty.");

                await status.UpdateStatusAsync();
                if (status.HasCompleted)
                    break;

                await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
            } while (!cancellationToken.IsCancellationRequested);

            if (cancellationToken.IsCancellationRequested)
                return Error.Unexpected("Email sending operation was cancelled.");

            return Unit.Value;

        }
    }
}
