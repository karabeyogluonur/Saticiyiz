namespace ST.Application.Constants
{
    public static class DataProtectionPurposes
    {
        /// <summary>
        /// Şifre sıfırlama için kullanılan Data Protection purpose
        /// </summary>
        public const string PasswordReset = "Satıcıyız.PasswordReset";

        /// <summary>
        /// Kullanıcı e-posta doğrulama için kullanılan Data Protection purpose
        /// </summary>
        public const string EmailVerification = "Satıcıyız.EmailVerification";

        /// <summary>
        /// İki faktörlü doğrulama (2FA) için kullanılan Data Protection purpose
        /// </summary>
        public const string TwoFactorAuthentication = "Satıcıyız.TwoFactorAuthentication";

        /// <summary>
        /// Bülten aboneliğinden ayrılmak için kullanılan Data Protection purpose
        /// </summary>
        public const string UnsubscribeNewsletter = "Satıcıyız.UnsubscribeNewsletter";
    }
}
