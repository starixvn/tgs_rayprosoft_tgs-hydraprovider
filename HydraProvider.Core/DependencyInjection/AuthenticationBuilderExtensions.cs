using System.Security.Claims;
using AspNetCore.Authentication.Basic;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace Tgs.HydraProvider.DependencyInjection;

public static class AuthenticationBuilderExtensions
{
	public static AuthenticationBuilder AddHydraProviderBasic(this AuthenticationBuilder builder)
		=> builder.AddBasic(options =>
		{
			options.Realm = "rpgp.cc";
			options.Events.OnValidateCredentials = async ctx =>
			{
				var loginChallenge = ctx.Request.Query["login_challenge"].FirstOrDefault();
				if (string.IsNullOrEmpty(loginChallenge))
				{
					ctx.ValidationFailed("login_challenge is required.");
					return;
				}

				var validateService = ctx.HttpContext.RequestServices.GetRequiredService<ValidateService>();

				var loginRequest = await validateService.GetLoginRequestAsync(
					loginChallenge,
					ctx.HttpContext.RequestAborted).ConfigureAwait(false);

				if (loginRequest is null)
				{
					ctx.ValidationFailed("Can't find login request.");
					return;
				}

				var system = loginRequest.GetSystem();

				if (system is null)
				{
					ctx.ValidationFailed("Can't find system scope");
					return;
				}

				var brandId = loginRequest.GetBrandId();
				if (brandId is null)
				{
					ctx.ValidationFailed("Can't get brand id.");
					return;
				}

				var validateResult = await validateService.ValidateAsync(
					system,
					brandId.Value,
					ctx.Username,
					ctx.Password,
					ctx.HttpContext.RequestAborted).ConfigureAwait(false);

				if (validateResult.IsValidate)
				{
					ctx.ValidationSucceeded(new[]
					{
						new Claim(
							ClaimTypes.Name,
							validateResult.Account!.UserName,
							ClaimValueTypes.String,
							options.ClaimsIssuer),
						new Claim(
							ClaimTypes.NameIdentifier,
							validateResult.Account!.UserName,
							ClaimValueTypes.String,
							options.ClaimsIssuer),
						new Claim(
							"system",
							system,
							ClaimValueTypes.String,
							options.ClaimsIssuer),
						new Claim(
							"brand_id",
							brandId.Value.ToString(),
							ClaimValueTypes.String,
							options.ClaimsIssuer),
						new Claim(
							"account_id",
							validateResult.Account.Id.ToString(),
							ClaimValueTypes.String,
							options.ClaimsIssuer)
					});
					return;
				}
				else
				{
					ctx.ValidationFailed("Validate failed.");
					return;
				}
			};
		});
}
