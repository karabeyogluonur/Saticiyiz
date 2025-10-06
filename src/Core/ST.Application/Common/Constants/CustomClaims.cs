namespace ST.Application.Common.Constants
{
    // Uygulamanın kullandığı özel Claim tiplerini tanımlar.
    public static class CustomClaims
    {
        public const string TenantId = "tenant_id";
        public const string Permission = "Permission"; // RoleClaims için ClaimType
    }
}