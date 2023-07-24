using System.Xml.Linq;
using FluentdForward.OpenTelemetry.MessagePack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Tgs.HydraProvider;
using Tgs.HydraProvider.DependencyInjection;
using Tgs.HydraProvider.ViewModels;
using Yarp.ReverseProxy.Forwarder;

var builder = WebApplication.CreateBuilder(args);

builder.Logging
	.AddOpenTelemetry(options =>
	{
		options.IncludeFormattedMessage = true;
		options.ParseStateValues = true;
		_ = options.AddFluentdForwardExporter(fluentOptions =>
		{
			var nodeIp = builder.Configuration.GetValue<string>("NODE_IP")!;
			fluentOptions.Host = nodeIp;
			fluentOptions.Tag = builder.Configuration.GetValue<string>("OTEL_SERVICE_NAME")!;
			fluentOptions.UseMessagePack(LogRecordFormatterResolver.GetResolverInstanceWithExtendInfo(
				"MachineName",
				() => Environment.MachineName));
		});
	});

builder.Services
	.AddAuthentication()
	.AddHydraProviderBasic()
	.Services
	.AddControllers()
	.Services
	.AddCors(cors => cors
		.AddDefaultPolicy(p => p.WithOrigins("*")))
	.AddAuthorization()
	.AddHydraProviders(httpBuilder => httpBuilder
		.ConfigureHttpClient(http => http.BaseAddress =
			builder.Configuration.GetValue<Uri>("Hydra:AdminUrl")!)
		.AddLoggingHandler<IHydraAdminClient>())
	.AddHydraProviderRepositories(dbBuilder => dbBuilder
		.UseNpgsql("name=Tgs"));

builder.Services
	.AddSwaggerGen(options => options.SwaggerDoc(
		"v1",
		new Microsoft.OpenApi.Models.OpenApiInfo { Title = "HydraProvider API", Version = "v1" }));

builder.Services
	.AddOpenTelemetry()
	.ConfigureResource(resourceBuilder => resourceBuilder
		.AddEnvironmentVariableDetector())
	.WithTracing(tracerBuilder => tracerBuilder
		.AddSource("Tgs.*")
		.AddSource("TGS.*")
		.AddAspNetCoreInstrumentation(options => options.Filter =
			httpContext => !httpContext.Request.Path.HasValue
				|| !httpContext.Request.Path.StartsWithSegments("/swagger")
				&& !httpContext.Request.Path.StartsWithSegments("/healthz")
				&& !httpContext.Request.Path.StartsWithSegments("/metrics")
				&& !new[] { "html", "ico", "png", "jpg" }.Any(
					ext => httpContext.Request.Path.Value.EndsWith($".{ext}")))
		.AddHttpClientInstrumentation()
		.AddProfileViewExporter()
		.AddOtlpExporter())
	.WithMetrics(meterBuilder => meterBuilder
		.AddRuntimeInstrumentation()
		.AddAspNetCoreInstrumentation()
		.AddHttpClientInstrumentation()
		.AddOtlpExporter())
	.Services
	.AddHealthChecks();

var app = builder.Build();

app.UseCors();
app.MapGet("/", () => "Hello World!");
//app.MapGet(
//	"/login",
//	async (
//		HttpContext httpContext,
//		[FromServices] IHydraAdminClient adminClient,
//		[FromServices] ILoginPageForwarder forwarder,
//		[FromQuery(Name = "login_challenge")] string loginChalleng) =>
//	{
//		var error = await forwarder.SendAsync(
//			httpContext,
//			"http://localhost:4200",
//			httpContext.RequestAborted).ConfigureAwait(false);

//		if (error != ForwarderError.None)
//		{
//			var errorFeature = httpContext.Features.Get<IForwarderErrorFeature>();
//			var exception = errorFeature?.Exception;
//		}

//		//var loginResponse = await adminClient.AcceptLoginAsync(
//		//	loginChalleng,
//		//	httpContext.User.Identity!.Name!,
//		//	new LoginContext(
//		//		Guid.Parse(httpContext.User.FindFirstValue("brand_id")!),
//		//		httpContext.User.FindFirstValue("system")!,
//		//		Guid.Parse(httpContext.User.FindFirstValue("account_id")!)),
//		//	httpContext.RequestAborted).ConfigureAwait(false);

//		//return Results.Redirect(loginResponse.RedirectTo.ToString());
//	});
//app.MapPost(
//	"/login",
//	async (
//		HttpContext httpContext,
//		[FromServices] IHydraAdminClient adminClient,
//		[FromQuery(Name = "login_challenge")] string loginChalleng,
//		LoginViewModel viewModel) =>
//	{
//		var loginRequest = await adminClient.GetLoginRequestAsync(
//			loginChalleng,
//			httpContext.RequestAborted).ConfigureAwait(false);


//	})
//	.Accepts<LoginViewModel>("application/x-www-form-urlencoded");

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
app.MapControllers();
app.MapHealthChecks("/healthz");
app.MapProfileViewer();
app.UseSwaggerUI();
app.MapSwagger();

app.Run();
