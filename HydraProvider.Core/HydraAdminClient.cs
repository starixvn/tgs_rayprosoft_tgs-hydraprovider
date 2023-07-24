using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace Tgs.HydraProvider;

internal class HydraAdminClient : IHydraAdminClient
{
	private static readonly JsonSerializerOptions _SerializerOptions = new(JsonSerializerDefaults.Web);
	private readonly HttpClient _httpClient;

	public HydraAdminClient(HttpClient httpClient)
	{
		_httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
	}

	public ValueTask<LoginRequest> GetLoginRequestAsync(string challenge, CancellationToken cancellationToken = default)
		=> _httpClient.GetAsync(
			$"/admin/oauth2/auth/requests/login{QueryString.Create("login_challenge", challenge)}",
			cancellationToken)
			.ToAsyncEnumerable()
			.Do(response => response.EnsureSuccessStatusCode())
			.SelectMany(response => response.Content
				.ReadFromJsonAsync<LoginRequest>(_SerializerOptions, cancellationToken)
				.ToAsyncEnumerable()
				.Where(reqData => reqData is not null)
				.Select(reqData => reqData!))
			.FirstAsync(cancellationToken);

	public ValueTask<ConsentRequest> GetConsentRequestAsync(string challenge, CancellationToken cancellationToken = default)
		=> _httpClient.GetAsync(
			$"/admin/oauth2/auth/requests/consent{QueryString.Create("consent_challenge", challenge)}",
			cancellationToken)
			.ToAsyncEnumerable()
			.Do(response => response.EnsureSuccessStatusCode())
			.SelectMany(response => response.Content
				.ReadFromJsonAsync<ConsentRequest>(_SerializerOptions, cancellationToken)
				.ToAsyncEnumerable()
				.Where(reqData => reqData is not null)
				.Select(reqData => reqData!))
			.FirstAsync(cancellationToken);

	public ValueTask<LoginResponse> AcceptLoginAsync(
		string challenge,
		string subject,
		LoginContext? loginContext = null,
		CancellationToken cancellationToken = default)
		=> _httpClient.PutAsJsonAsync(
			$"/admin/oauth2/auth/requests/login/accept{QueryString.Create("login_challenge", challenge)}",
			new AcceptLoginRequest(
				subject,
				loginContext),
			cancellationToken)
			.ToAsyncEnumerable()
			.Do(response => response.EnsureSuccessStatusCode())
			.SelectMany(response => response.Content
				.ReadFromJsonAsync<LoginResponse>(_SerializerOptions, cancellationToken)
				.ToAsyncEnumerable()
				.Where(resData => resData is not null)
				.Select(resData => resData!))
			.FirstAsync(cancellationToken);

	public ValueTask<ConsentResponse> AcceptConsentAsync(
		string challenge,
		AcceptConsentRequest acceptConsentRequest,
		CancellationToken cancellationToken = default)
		=> _httpClient.PutAsJsonAsync(
			$"/admin/oauth2/auth/requests/consent/accept{QueryString.Create("consent_challenge", challenge)}",
			acceptConsentRequest,
			cancellationToken)
			.ToAsyncEnumerable()
			.Do(response => response.EnsureSuccessStatusCode())
			.SelectMany(response => response.Content
				.ReadFromJsonAsync<ConsentResponse>(_SerializerOptions, cancellationToken)
				.ToAsyncEnumerable()
				.Where(resData => resData is not null)
				.Select(resData => resData!))
			.FirstAsync(cancellationToken);

	public ValueTask<LoginResponse> RejectLoginAsync(string challenge, CancellationToken cancellationToken = default)
		=> _httpClient.PutAsync(
			$"/admin/oauth2/auth/requests{QueryString.Create("login_challenge", challenge)}",
			null,
			cancellationToken)
			.ToAsyncEnumerable()
			.Do(response => response.EnsureSuccessStatusCode())
			.SelectMany(response => response.Content
				.ReadFromJsonAsync<LoginResponse>(_SerializerOptions, cancellationToken)
				.ToAsyncEnumerable()
				.Where(resData => resData is not null)
				.Select(resData => resData!))
			.FirstAsync(cancellationToken);
}
