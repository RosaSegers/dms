namespace User.API.Common.Interfaces
{
    public interface ICurrentUserService
    {
        Guid? UserId { get; }
    }
}
