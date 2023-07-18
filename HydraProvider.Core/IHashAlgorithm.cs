namespace Tgs.HydraProvider;

public interface IHashAlgorithm
{
	ValueTask<bool> ValidateAsync(string password, string passwordHash, CancellationToken cancellationToken = default);

	ValueTask<string> ComputeHashAsync(string password, CancellationToken cancellationToken = default);
}
