using System.Threading.Tasks;

namespace ST.Application.Interfaces.Configuration
{
    public interface IDbInitializer
    {
        Task InitializeAsync();
    }
}
