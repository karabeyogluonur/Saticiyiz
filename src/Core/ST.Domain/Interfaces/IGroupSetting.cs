using System.Reflection;

namespace ST.Domain.Interfaces
{
    // ISetting Marker Interface'inden türetildi.
    // Bu, ayar nesnesinin veritabanı kaydında bir önek (prefix) kullanacağını belirtir.
    public interface IGroupSetting : ISetting
    {
        string GetPrefix();
    }
}