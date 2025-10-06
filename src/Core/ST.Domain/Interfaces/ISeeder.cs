using System.Threading.Tasks;

namespace ST.Domain.Interfaces
{
    // Tüm seed operasyonlarının temel arayüzü
    public interface ISeeder
    {
        Task SeedAsync();
    }
}