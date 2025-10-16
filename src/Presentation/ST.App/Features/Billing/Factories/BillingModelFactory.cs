using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc.Rendering;
using ST.App.Features.Billing.ViewModels;
using ST.Application.Extensions;
using ST.Application.Features.Billing.Queries;
using ST.Application.Features.Billing.Queries.GetAllBillingProfile;
using ST.Application.Features.Billing.Queries.GetBillingProfileById;
using ST.Application.Interfaces.Common;
using ST.Domain.Enums;



namespace ST.App.Features.Billing.Factories
{
    public class BillingModelFactory : IBillingModelFactory
    {
        private readonly ILookupService _lookupService;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public BillingModelFactory(ILookupService lookupService, IMediator mediator, IMapper mapper)
        {
            _lookupService = lookupService;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<BillingProfileAddViewModel> PrepareAddViewModelAsync()
        {
            var model = new BillingProfileAddViewModel();
            await PopulateViewModelListsAsync(model);
            return model;
        }

        public async Task<BillingProfileAddViewModel> PrepareAddViewModelAsync(BillingProfileAddViewModel model)
        {
            await PopulateViewModelListsAsync(model);
            return model;
        }



        public async Task<BillingProfileListViewModel> PrepareListViewModelAsync()
        {
            var viewModel = new BillingProfileListViewModel();

            var queryResult = await _mediator.Send(new GetAllBillingProfilesQuery());

            if (queryResult.Succeeded && queryResult.Data.Any())
            {
                foreach (var dto in queryResult.Data)
                {
                    var item = new BillingProfileItemViewModel
                    {
                        Id = dto.Id,
                        IsPrimary = dto.IsPrimary,
                        FullAddress = $"{dto.Address}<br />{dto.DistrictName} / {dto.CityName}<br />{dto.PostalCode}",
                    };

                    if (dto.BillingAccountType == BillingAccountType.Individual)
                    {
                        item.DisplayName = $"{dto.IndividualFirstName} {dto.IndividualLastName}";
                        item.AccountTypeDisplayName = "Bireysel";
                        item.BadgeClass = "badge-light-primary";
                    }
                    else
                    {
                        item.DisplayName = dto.CorporateCompanyName;
                        item.AccountTypeDisplayName = "Kurumsal";
                        item.BadgeClass = "badge-light-success";
                    }
                    viewModel.BillingProfiles.Add(item);
                }
            }

            return viewModel;

        }



        public async Task<BillingProfileEditViewModel> PrepareEditViewModelAsync(int id)
        {
            var result = await _mediator.Send(new GetBillingProfileByIdQuery { Id = id });
            if (!result.Succeeded)
                return new BillingProfileEditViewModel();

            var model = _mapper.Map<BillingProfileEditViewModel>(result.Data);
            await PopulateViewModelListsAsync(model);
            return model;
        }

        public async Task<BillingProfileEditViewModel> PrepareEditViewModelAsync(BillingProfileEditViewModel model)
        {
            await PopulateViewModelListsAsync(model);
            return model;
        }

        private async Task PopulateViewModelListsAsync<T>(T model) where T : class
        {
            var billingAccountTypeProp = typeof(T).GetProperty("BillingAccountTypeList");
            var cityListProp = typeof(T).GetProperty("CityList");
            var districtListProp = typeof(T).GetProperty("DistrictList");
            var cityIdProp = typeof(T).GetProperty("CityId");
            var districtIdProp = typeof(T).GetProperty("DistrictId");

            if (billingAccountTypeProp != null)
                billingAccountTypeProp.SetValue(model, EnumExtensions.ToSelectList<BillingAccountType>());

            if (cityListProp != null)
            {
                var cities = await _lookupService.GetCitiesAsync();
                cityListProp.SetValue(model, new SelectList(cities, "Id", "Name"));
            }

            if (districtListProp != null)
            {
                var cityId = (int)(cityIdProp?.GetValue(model) ?? 0);
                if (cityId > 0)
                {
                    var districts = await _lookupService.GetDistrictsByCityAsync(cityId);
                    var districtId = (int)(districtIdProp?.GetValue(model) ?? 0);
                    districtListProp.SetValue(model, new SelectList(districts, "Id", "Name", districtId));
                }
            }
        }
    }
}