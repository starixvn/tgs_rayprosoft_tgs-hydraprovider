using System.Text.Json.Serialization;

namespace Tgs.HydraProvider;

public record OAuthClient(
	[property: JsonPropertyName("client_id")] string ClientId,
	[property: JsonPropertyName("scope")] string Scopes);
