namespace ST.Domain.Interfaces
{
    public interface ITenantInfo
    {
        int Id { get; }
        string Identifier { get; }
        string Name { get; }
    }
}
