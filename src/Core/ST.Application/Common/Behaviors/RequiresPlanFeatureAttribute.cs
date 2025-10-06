namespace ST.Application.Common.Behaviors;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public class RequiresPlanFeatureAttribute : Attribute
{
    public string FeatureKey { get; }
    public RequiresPlanFeatureAttribute(string featureKey) => FeatureKey = featureKey;
}