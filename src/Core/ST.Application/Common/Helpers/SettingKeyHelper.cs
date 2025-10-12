using ST.Domain.Interfaces;
using System.Linq.Expressions;
using System.Reflection;

namespace ST.Application.Common.Helpers
{
    public static class SettingKeyHelper
    {
        public static string GetKey<TSetting, TProperty>(Expression<Func<TSetting, TProperty>> expression)
            where TSetting : ISetting, new()
        {
            if (expression.Body is not MemberExpression memberExpression)
            {
                throw new ArgumentException("Expression must be a member expression (e.g., settings => settings.PropertyName).", nameof(expression));
            }

            var propertyInfo = memberExpression.Member as PropertyInfo;
            if (propertyInfo == null)
            {
                throw new ArgumentException("Expression must refer to a property.", nameof(expression));
            }

            var settingInstance = new TSetting();
            string prefix = (settingInstance is IGroupSetting groupSetting)
                ? groupSetting.GetPrefix() + "."
                : string.Empty;

            return prefix + propertyInfo.Name;
        }
    }
}