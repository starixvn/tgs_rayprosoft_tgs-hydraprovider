namespace Tgs.HydraProvider;

public interface IValidateService
{
	ValueTask<LoginRequest> GetLoginRequestAsync(string loginChallenge, CancellationToken cancellationToken = default);

	ValueTask<(bool IsValidate, Account? Account)> ValidateAsync(
		string system,
		Guid brandId,
		string userName,
		string password,
		CancellationToken cancellationToken = default);

	ValueTask<LoginResponse> AcceptLoginAsync(
		string loginChallenge,
		string subject,
		LoginContext? loginContext,
		CancellationToken cancellationToken = default);

	ValueTask<LoginResponse> RejectLoginAsync(
		string loginChallenge,
		CancellationToken cancellationToken = default);
}
