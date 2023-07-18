using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tgs.HydraProvider;
using Tgs.HydraProvider.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services
	.AddAuthentication()
	.AddHydraProviderBasic()
	.Services
	.AddAuthorization()
	.AddHydraProviders(httpBuilder => httpBuilder
		.ConfigureHttpClient(http => http.BaseAddress =
			builder.Configuration.GetValue<Uri>("Hydra:AdminUrl")!)
		.AddLoggingHandler<IHydraAdminClient>())
	.AddHydraProviderRepositories(dbBuilder => dbBuilder
		.UseNpgsql("name=Tgs"));

var app = builder.Build();

app.MapGet("/", () => "Hello World!");
app.MapGet(
	"/login",
	async (
		HttpContext httpContext,
		[FromServices] IHydraAdminClient adminClient,
		[FromQuery(Name = "login_challenge")] string loginChalleng) =>
	{
		var loginResponse = await adminClient.AcceptLoginAsync(
			loginChalleng,
			httpContext.User.Identity!.Name!,
			new LoginContext(
				Guid.Parse(httpContext.User.FindFirstValue("brand_id")!),
				httpContext.User.FindFirstValue("system")!,
				Guid.Parse(httpContext.User.FindFirstValue("account_id")!)),
			httpContext.RequestAborted).ConfigureAwait(false);

		return Results.Redirect(loginResponse.RedirectTo.ToString());
	})
	.RequireAuthorization();

app.MapGet(
	"/consent",
	async (
		HttpContext httpContext,
		[FromServices] IHydraAdminClient adminClient,
		[FromQuery(Name = "consent_challenge")] string consentChallenge) =>
	{
		var consentRequest = await adminClient.GetConsentRequestAsync(
			consentChallenge,
			httpContext.RequestAborted).ConfigureAwait(false);

		var consentResponse = await adminClient.AcceptConsentAsync(
			consentChallenge,
			new AcceptConsentRequest(
				null,
				consentRequest.RequestedScopes,
				consentRequest.CreateTokenSession()),
			httpContext.RequestAborted).ConfigureAwait(false);

		return Results.Redirect(consentResponse.RedirectTo.ToString());
	});
app.Run();
