using System.Collections.Generic;

namespace ST.App.Features.Billing.ViewModels
{
    public class BillingProfileListViewModel
    {
        public List<BillingProfileItemViewModel> BillingProfiles { get; set; } = new List<BillingProfileItemViewModel>();
    }

    public class BillingProfileItemViewModel
    {
        public int Id { get; set; }
        public bool IsPrimary { get; set; }
        public string AccountTypeDisplayName { get; set; } = string.Empty;
        public string BadgeClass { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string FullAddress { get; set; } = string.Empty;
    }
}