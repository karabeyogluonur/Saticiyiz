namespace ST.Application.Interfaces.Contexts
{
    /// <summary>
    /// Kiracıya özel veritabanında bulunan varlıklar için bir sözleşme tanımlar.
    /// </summary>
    public interface ITenantDbContext
    {
        // Gelecekte eklenecek diğer özel DbSet'ler...
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}