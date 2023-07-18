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
			.AddSingleton<IHashAlgorithmFactory, HashAlgorithmFactory>()
			.AddTransient<ValidateService>();
	}
}
