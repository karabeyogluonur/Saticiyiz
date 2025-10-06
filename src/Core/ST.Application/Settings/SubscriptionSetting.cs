

using ST.Domain.Entities.Common;
using ST.Domain.Interfaces;

namespace ST.Application.Settings
{
    // Bu nesne, Abonelik ile ilgili tüm ayar değerlerini tutar.
    public class SubscriptionSetting : IGroupSetting
    {
        public SubscriptionSetting()
        {
            TrialPeriodDays = 7;
        }
        public string GetPrefix() => "Subscription"; // İstenen prefix: "Subscription."

        public int TrialPeriodDays { get; set; }
        public int MaxStorageLimitGb { get; set; }
    }
}