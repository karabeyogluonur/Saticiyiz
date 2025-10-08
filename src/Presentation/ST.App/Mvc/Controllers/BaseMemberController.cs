using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ST.App.Mvc.Controllers;

[Authorize]
public class BaseMemberController : BaseController
{

}
