namespace Tgs.HydraProvider;

public interface ITgsRepository
{
	ValueTask<Account?> GetLoginAccountAsync(
		Guid brandId,
		string system,
		string userName,
		CancellationToken cancellationToken = default);

	ValueTask AddAccountEventAsync(
		AccountEvent accountEvent,
		CancellationToken cancellationToken = default);

	ValueTask UpsertAccountAsync(
		AccountSnapshot snapshot,
		Guid snapshotId,
		DateTime updatedOn,
		CancellationToken cancellationToken = default);
}
