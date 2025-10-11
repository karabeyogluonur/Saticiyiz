namespace ST.Domain.Entities.Common
{
    public interface ISoftDeleteEntity
    {
        bool IsDeleted { get; set; }
        DateTime? DeletedDate { get; set; }
        string? DeletedBy { get; set; }
    }
}
