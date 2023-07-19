using System.Text.Json;
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

	public async ValueTask AddAccountEventAsync(AccountEvent accountEvent, CancellationToken cancellationToken = default)
	{
		_ = await _dbContext.AccountEventSources.AddAsync(
			new AccountEventSource
			{
				Id = accountEvent.Id,
				AccountId = accountEvent.AccountId,
				ChangeSets = JsonSerializer.Serialize(accountEvent.ChangeSets),
				CreatedOn = accountEvent.CreatedOn
			},
			cancellationToken).ConfigureAwait(false);

		_ = await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
	}

	public async ValueTask UpsertAccountAsync(
		AccountSnapshot snapshot,
		Guid snapshotId,
		DateTime updatedOn,
		CancellationToken cancellationToken = default)
	{
		var row = (await _dbContext.AccountRows.FindAsync(
			new object?[] { snapshot.Id },
			cancellationToken: cancellationToken).ConfigureAwait(false))
			?? (await _dbContext.AccountRows.AddAsync(
				new AccountRow
				{
					Id = snapshot.Id
				},
				cancellationToken).ConfigureAwait(false)).Entity;
		row.BrandId = snapshot.BrandId;
		row.System = snapshot.System;
		row.Algorithm = snapshot.Algorithm;
		row.UserName = snapshot.UserName;
		row.PasswordHash = snapshot.PasswordHash;
		row.CreatedOn = snapshot.CreatedOn.ToUniversalTime();
		row.UpdatedOn = updatedOn.ToUniversalTime();
		row.SnapshotId = snapshotId;

		_ = await _dbContext.SaveChangesAsync(
			   cancellationToken).ConfigureAwait(false);
	}
}
