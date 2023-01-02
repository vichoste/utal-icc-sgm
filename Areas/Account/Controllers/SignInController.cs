using Microsoft.AspNetCore.Mvc;

namespace Utal.Icc.Sgm.Areas.Account.Controllers;

public class SignInController : Controller {
	public IActionResult Index() => this.View();
}