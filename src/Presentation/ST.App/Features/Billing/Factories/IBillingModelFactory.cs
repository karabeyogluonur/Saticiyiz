using ST.App.Features.Billing.ViewModels;

namespace ST.App.Features.Billing.Factories;

public interface IBillingModelFactory
{
    Task<BillingProfileAddViewModel> PrepareAddViewModelAsync();
    Task<BillingProfileAddViewModel> PrepareAddViewModelAsync(BillingProfileAddViewModel model);


    Task<BillingProfileListViewModel> PrepareListViewModelAsync();


    Task<BillingProfileEditViewModel> PrepareEditViewModelAsync(int id);
    Task<BillingProfileEditViewModel> PrepareEditViewModelAsync(BillingProfileEditViewModel model);
}