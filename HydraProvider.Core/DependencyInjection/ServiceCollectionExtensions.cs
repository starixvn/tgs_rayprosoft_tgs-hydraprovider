using System.Reflection;
using Tgs.HydraProvider;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddHydraProviders(
		this IServiceCollection services,
		Action<IHttpClientBuilder> configureHttpClient)
	{
		configureHttpClient?.Invoke(services
			.AddHttpClient<IHydraAdminClient, HydraAdminClient>());

		return services
			.AddReverseProxy()
			.Services
			.AddOptions<HydraProviderOptions>()
			.Services
			.RegisterAllServiceUtilityTypes(Assembly.GetExecutingAssembly(), ServiceLifetime.Transient)
			.AddSingleton<IHashAlgorithmFactory, HashAlgorithmFactory>()
			.AddTransient<IValidateService, ValidateService>()
			.AddSerialNumberGenerator()
			.AddSingleton<LoginPageTransformer>()
			.AddSingleton<LoginPageConfiguration>()
			.AddScoped<ILoginPageForwarder, LoginPageForwarder>();
	}
}
