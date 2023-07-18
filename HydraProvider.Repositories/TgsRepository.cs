using Microsoft.EntityFrameworkCore;
using Tgs.HydraProvider.Repositories.Entities;

namespace Tgs.HydraProvider.Repositories;

internal class TgsRepository : ITgsRepository
{
	private readonly TgsContext _dbContext;

	public TgsRepository(TgsContext dbContext)
	{
		_dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
	}

	public async ValueTask<Account?> GetLoginAccountAsync(
		Guid brandId,
		string system,
		string userName,
		CancellationToken cancellationToken = default)
	{
		var row = await _dbContext.AccountRows
			.Where(a => a.BrandId == brandId
				&& a.System == system
				&& a.UserName == userName)
			.SingleOrDefaultAsync(cancellationToken)
			.ConfigureAwait(false);

		return row is null
			? null
			: new Account(
				row.Id,
				row.UserName,
				row.BrandId,
				row.System,
				row.Algorithm,
				row.PasswordHash);
	}
}
