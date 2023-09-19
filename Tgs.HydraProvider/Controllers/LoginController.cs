using Microsoft.AspNetCore.Mvc;
using Tgs.HydraProvider.ViewModels;

namespace Tgs.HydraProvider.Controllers;

[Route("[controller]")]
[ApiController]
public class LoginController : ControllerBase
{
	[HttpGet(Name = "GetLoginForm")]
	public Task<string> GetLoginFormAsync(
		//[FromServices] ILoginPageForwarder forwarder,
		[FromQuery(Name = "login_challenge")] string loginChallenge)
	{
		return Task.FromResult(loginChallenge);
		//var error = await forwarder.SendAsync(
		//	HttpContext,
		//	"https://z9p0wqvz-7074.asse.devtunnels.ms",
		//	HttpContext.RequestAborted).ConfigureAwait(false);

		//if (error != ForwarderError.None)
		//{
		//	var errorFeature = HttpContext.Features.Get<IForwarderErrorFeature>();
		//	var exception = errorFeature?.Exception;

		//	if (exception is not null)
		//		throw exception;
		//}

		//return Task.FromResult<string>();
	}

	[HttpPost(Name = "PostLogin")]
	public async Task<IResult> PostLoginAsync(
		[FromServices] IValidateService validateService,
		[FromQuery(Name = "login_challenge")] string loginChalleng,
		[FromBody] LoginRequestViewModel viewModel)
	{
		var loginRequest = await validateService.GetLoginRequestAsync(
			loginChalleng,
			HttpContext.RequestAborted).ConfigureAwait(false);

		if (loginRequest is null)
			return Results.BadRequest("Can't find login request.");

		var validateResult = await validateService.ValidateAsync(
			loginRequest.Client.ClientName,
			viewModel.BrandId,
			viewModel.UserName,
			viewModel.Password,
			HttpContext.RequestAborted).ConfigureAwait(false);

		if (validateResult.IsValidate)
		{
			var loginResponse = await validateService.AcceptLoginAsync(
				loginChalleng,
				validateResult.Account!.UserName,
				new LoginContext(
					viewModel.BrandId,
					loginRequest.Client.ClientName,
					validateResult.Account.Id),
				HttpContext.RequestAborted).ConfigureAwait(false);

			return Results.Redirect(loginResponse.RedirectTo.ToString());
		}
		else
		{
			var loginResponse = await validateService.RejectLoginAsync(
				loginChalleng,
				HttpContext.RequestAborted).ConfigureAwait(false);

			return Results.Redirect(loginResponse.RedirectTo.ToString());
		}
	}
}
