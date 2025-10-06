using System.Reflection;

namespace ST.Domain.Interfaces
{
    public interface IGroupSetting : ISetting
    {
        string GetPrefix();
    }
}