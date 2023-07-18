using Microsoft.EntityFrameworkCore;
using Tgs.HydraProvider;
using Tgs.HydraProvider.Repositories;
using Tgs.HydraProvider.Repositories.Entities;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddHydraProviderRepositories(
		this IServiceCollection services,
		Action<DbContextOptionsBuilder> dbConfigure)
		=> services
			.AddDbContext<TgsContext>(dbConfigure)
			.AddTransient<ITgsRepository, TgsRepository>();
}
