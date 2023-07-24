namespace Tgs.HydraProvider;

internal class ValidateService : IValidateService
{
	private readonly IHydraAdminClient _hydraAdminClient;
	private readonly ITgsRepository _tgsRepository;
	private readonly IHashAlgorithmFactory _hashAlgorithmFactory;

	public ValidateService(IHydraAdminClient hydraAdminClient, ITgsRepository tgsRepository, IHashAlgorithmFactory hashAlgorithmFactory)
	{
		_hydraAdminClient = hydraAdminClient ?? throw new ArgumentNullException(nameof(hydraAdminClient));
		_tgsRepository = tgsRepository ?? throw new ArgumentNullException(nameof(tgsRepository));
		_hashAlgorithmFactory = hashAlgorithmFactory ?? throw new ArgumentNullException(nameof(hashAlgorithmFactory));
	}

	public ValueTask<LoginRequest> GetLoginRequestAsync(string loginChallenge, CancellationToken cancellationToken = default)
		=> _hydraAdminClient.GetLoginRequestAsync(loginChallenge, cancellationToken);

	public async ValueTask<(bool IsValidate, Account? Account)> ValidateAsync(
		string system,
		Guid brandId,
		string userName,
		string password,
		CancellationToken cancellationToken = default)
	{
		var account = await _tgsRepository.GetLoginAccountAsync(
			brandId,
			system,
			userName,
			cancellationToken).ConfigureAwait(false);

		if (account is null)
			return (false, null);

		var hasher = _hashAlgorithmFactory.GetHashAlgorithm(account.Algorithm);

		return (
			await hasher.ValidateAsync(
				password,
				account.PasswordHash,
				cancellationToken).ConfigureAwait(false),
			account);
	}

	public ValueTask<LoginResponse> AcceptLoginAsync(
		string loginChallenge,
		string subject,
		LoginContext? loginContext,
		CancellationToken cancellationToken = default)
		=> _hydraAdminClient.AcceptLoginAsync(
			loginChallenge,
			subject,
			loginContext,
			cancellationToken);

	public ValueTask<LoginResponse> RejectLoginAsync(string loginChallenge, CancellationToken cancellationToken = default)
		=> _hydraAdminClient.RejectLoginAsync(loginChallenge, cancellationToken);
}
