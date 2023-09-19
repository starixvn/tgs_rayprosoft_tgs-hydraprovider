using Microsoft.AspNetCore.Mvc;
using Yarp.ReverseProxy.Forwarder;

namespace Tgs.HydraProvider.Controllers;

[Route("[controller]")]
[ApiController]
public class LoginController : ControllerBase
{
	[HttpGet(Name = "GetLoginForm")]
	public Task<string> GetLoginFormAsync(
		[FromServices] ILoginPageForwarder forwarder,
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
