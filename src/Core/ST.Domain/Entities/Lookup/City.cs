using ST.Domain.Entities.Common;

namespace ST.Domain.Entities.Lookup
{
    public class City : BaseEntity<int>
    {
        public string Name { get; set; } = null!;
        public ICollection<District> Districts { get; set; } = new List<District>();
    }
}
