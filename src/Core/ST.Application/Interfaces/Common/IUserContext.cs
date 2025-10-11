public interface IUserContext
{
    bool IsAuthenticated { get; }
    int UserId { get; }
    string Username { get; }
}
