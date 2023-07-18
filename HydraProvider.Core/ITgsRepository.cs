namespace Tgs.HydraProvider;

public interface ITgsRepository
{
	ValueTask<Account?> GetLoginAccountAsync(
		Guid brandId,
		string system,
		string userName,
		CancellationToken cancellationToken = default);
}
