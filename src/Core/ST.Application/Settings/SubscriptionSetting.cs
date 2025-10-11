

using ST.Domain.Entities.Common;
using ST.Domain.Interfaces;

namespace ST.Application.Settings
{
    public class SubscriptionSetting : IGroupSetting
    {
        public SubscriptionSetting()
        {
            TrialPeriodDays = 7;
            MaxStorageLimitGb = 10;
        }
        public string GetPrefix() => "Subscription";

        public int TrialPeriodDays { get; set; }
        public int MaxStorageLimitGb { get; set; }
    }
}
