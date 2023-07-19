using Microsoft.AspNetCore.Mvc;
using Tgs.HydraProvider.ApplicationServices;
using Tgs.HydraProvider.ViewModels;
using Tgs.ServiceUtility;

namespace Tgs.HydraProvider.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
	[HttpPut(Name = "CreateAccount")]
	public ValueTask CreateAccountAsync(
		[FromServices] ICommandService<CreateAccountCommand> createService,
		CreateAccountViewModel viewModel)
	{
		var command = new CreateAccountCommand(
			viewModel.System,
			viewModel.BrandId,
			viewModel.UserName,
			viewModel.Password);

		return createService.ExecuteAsync(command, HttpContext.RequestAborted);
	}
}
