namespace Tgs.HydraProvider;

public interface IHydraAdminClient
{
	ValueTask<ConsentResponse> AcceptConsentAsync(
		string challenge,
		AcceptConsentRequest acceptConsentRequest,
		CancellationToken cancellationToken = default);

	ValueTask<LoginResponse> AcceptLoginAsync(
		string challenge,
		string subject,
		LoginContext? loginContext = null,
		CancellationToken cancellationToken = default);

	ValueTask<LoginResponse> RejectLoginAsync(
		string challenge,
		CancellationToken cancellationToken = default);

	ValueTask<ConsentRequest> GetConsentRequestAsync(string challenge, CancellationToken cancellationToken = default);

	ValueTask<LoginRequest> GetLoginRequestAsync(string challenge, CancellationToken cancellationToken = default);
}
