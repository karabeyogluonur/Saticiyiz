using System.Threading.Tasks;

namespace ST.Domain.Interfaces
{
    public interface ISeeder
    {
        Task SeedAsync();
    }
}