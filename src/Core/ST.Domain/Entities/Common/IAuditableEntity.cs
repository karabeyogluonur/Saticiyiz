namespace ST.Domain.Entities.Common
{
    public interface IAuditableEntity
    {
        string CreatedBy { get; set; }
        DateTime CreatedDate { get; set; }
        string? LastModifiedBy { get; set; }
        DateTime? LastModifiedDate { get; set; }
    }
}