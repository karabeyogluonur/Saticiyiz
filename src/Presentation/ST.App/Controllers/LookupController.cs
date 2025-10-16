using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ST.App.Mvc.Controllers;
using ST.Application.Interfaces.Common;

namespace ST.App.Controllers
{
    public class LookupController : BaseController
    {
        private readonly ILookupService _lookupService;

        public LookupController(ILookupService lookupService)
        {
            _lookupService = lookupService;
        }

        [HttpGet]
        public async Task<IActionResult> GetDistricts(int cityId)
        {
            if (cityId <= 0)
                return Json(new SelectList(Enumerable.Empty<SelectListItem>()));

            var districts = await _lookupService.GetDistrictsByCityAsync(cityId);
            var selectList = new SelectList(districts, "Id", "Name");
            return Json(selectList);
        }
    }
}