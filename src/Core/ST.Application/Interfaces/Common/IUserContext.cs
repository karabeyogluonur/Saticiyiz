public interface IUserContext
{
    bool IsAuthenticated { get; }
    int UserId { get; }
    string EmailOrUsername { get; }
    int TenantId { get; }

}
