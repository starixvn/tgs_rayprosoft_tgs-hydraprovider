using Tgs.SerialNumberGenerator;
using Tgs.ServiceUtility;

namespace Tgs.HydraProvider.ApplicationServices;

internal class CreateAccountCommandService : ICommandService<CreateAccountCommand>
{
	private readonly ITgsRepository _tgsRepository;
	private readonly IHashAlgorithmFactory _hashAlgorithmFactory;
	private readonly ISerialNumberGenerator _serialNumberGenerator;

	public CreateAccountCommandService(
		ITgsRepository tgsRepository,
		IHashAlgorithmFactory hashAlgorithmFactory,
		ISerialNumberGenerator serialNumberGenerator)
	{
		_tgsRepository = tgsRepository ?? throw new ArgumentNullException(nameof(tgsRepository));
		_hashAlgorithmFactory = hashAlgorithmFactory ?? throw new ArgumentNullException(nameof(hashAlgorithmFactory));
		_serialNumberGenerator = serialNumberGenerator ?? throw new ArgumentNullException(nameof(serialNumberGenerator));
	}

	public async ValueTask ExecuteAsync(CreateAccountCommand command, CancellationToken cancellationToken = default)
	{
		var hasher = _hashAlgorithmFactory.GetDefaultHashAlgorithm();

		var accountSnapshot = new AccountSnapshot(
			Guid.NewGuid(),
			command.UserName,
			command.BrandId,
			command.System,
			_hashAlgorithmFactory.DefaultAlgorithmName,
			await hasher.ComputeHashAsync(command.Password, cancellationToken).ConfigureAwait(false),
			DateTime.UtcNow);

		var accountEvent = accountSnapshot.CreateChangeEvent(
			await _serialNumberGenerator.GenerateAsync(
				cancellationToken).ConfigureAwait(false),
			null,
			accountSnapshot.CreatedOn);

		await _tgsRepository.AddAccountEventAsync(
			accountEvent,
			cancellationToken).ConfigureAwait(false);

		await _tgsRepository.UpsertAccountAsync(
			accountSnapshot,
			accountEvent.Id,
			accountEvent.CreatedOn,
			cancellationToken).ConfigureAwait(false);
	}
}
