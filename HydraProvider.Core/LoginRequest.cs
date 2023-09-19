using System.Text.Json.Serialization;

namespace Tgs.HydraProvider;

public record LoginRequest(
	string Challenge,
	[property: JsonPropertyName("requested_scope")] string[] RequestedScopes,
	bool Skip,
	string Subject,
	OAuthClient Client,
	[property: JsonPropertyName("request_url")] Uri RequestUri,
	[property: JsonPropertyName("session_id")] string SessionId);
