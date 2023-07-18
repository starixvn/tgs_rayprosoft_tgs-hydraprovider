using System.Text.Json.Serialization;
using Tgs.HydraProvider;

namespace Tgs.HydraProvider;

public record AcceptConsentRequest(
	[property: JsonPropertyName("grant_access_token_audience")] string[]? GrantAccessTokenAudiences,
	[property: JsonPropertyName("grant_scope")] string[]? GrantScopes,
	TokenSession? Session);
