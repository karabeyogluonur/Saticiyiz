namespace ST.Application.Interfaces.Identity
{
    public interface ICurrentUserService
    {
        string? UserId { get; }
    }
}