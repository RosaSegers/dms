namespace AccessControl.Api.Common.Interfaces
{
    public interface ICurrentUserService
    {
        string? UserId { get; }
    }
}
