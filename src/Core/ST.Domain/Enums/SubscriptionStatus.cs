namespace ST.Domain.Enums
{
    public enum SubscriptionStatus
    {
        // Yeni kiracılar için başlangıç durumu
        Trial = 10,

        // Ödeme periyodu aktif olanlar
        Active = 20,

        // Deneme süresi/ödeme periyodu bitmiş, erişimi kısıtlanmış
        Expired = 30,

        // Kiracı tarafından iptal edilmiş veya ödeme alınamamış
        Canceled = 40
    }
}