namespace ST.Domain.Constants
{
    public static class AppPermissions
    {
        // Kiracı Yönetimi (Admin Yetkisi)
        public const string UserManagement_Create = "UserManagement.Create";
        public const string RoleAssignment_Manage = "RoleAssignment.Manage"; // İzin/Rol atamalarını yönetme
        public const string Subscription_Manage = "Subscription.Manage";

        // Temel İşlem Yetkileri (Member/Admin Ortak)
        public const string Dashboard_View = "Dashboard.View";
        public const string Projects_Create = "Projects.Create";
        public const string Reports_View = "Reports.View";
    }
}