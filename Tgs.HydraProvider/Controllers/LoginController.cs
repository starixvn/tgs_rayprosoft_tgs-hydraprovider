using Microsoft.AspNetCore.Mvc;
using Yarp.ReverseProxy.Forwarder;

namespace Tgs.HydraProvider.Controllers;

[Route("[controller]")]
[ApiController]
public class LoginController : ControllerBase
{
	[HttpGet(Name = "GetLoginForm")]
	public async Task GetLoginFormAsync(
		[FromServices] ILoginPageForwarder forwarder)
	{
		var error = await forwarder.SendAsync(
			HttpContext,
			"http://localhost:4200",
			HttpContext.RequestAborted).ConfigureAwait(false);

		if (error != ForwarderError.None)
		{
			var errorFeature = HttpContext.Features.Get<IForwarderErrorFeature>();
			var exception = errorFeature?.Exception;
		}
	}

	[HttpPost(Name = "PostLogin")]
	[Consumes("application/x-www-form-urlencoded")]
	[Produces("text/html")]
	public async Task<IResult> PostLoginAsync(
		[FromServices] IValidateService validateService,
		[FromQuery(Name = "login_challenge")] string loginChalleng,
		[FromForm(Name = "userName")] string userName,
		[FromForm(Name = "password")] string password)
	{
		var loginRequest = await validateService.GetLoginRequestAsync(
			loginChalleng,
			HttpContext.RequestAborted).ConfigureAwait(false);

		if (loginRequest is null)
			return Results.BadRequest("Can't find login request.");

		var system = loginRequest.GetSystem();

		if (system is null)
			return Results.BadRequest("Can't find system scope");

		var brandId = loginRequest.GetBrandId();
		if (brandId is null)
			return Results.BadRequest("Can't get brand id.");

		var validateResult = await validateService.ValidateAsync(
			system,
			brandId.Value,
			userName,
			password,
			HttpContext.RequestAborted).ConfigureAwait(false);

		if (validateResult.IsValidate)
		{
			var loginResponse = await validateService.AcceptLoginAsync(
				loginChalleng,
				validateResult.Account!.UserName,
				new LoginContext(
					brandId.Value,
					system,
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
