using System.Text.Json.Serialization;

namespace Tgs.HydraProvider;

public record OAuthClient(
	[property: JsonPropertyName("client_id")] string ClientId,
	[property: JsonPropertyName("client_name")] string ClientName,
	[property: JsonPropertyName("scope")] string Scopes);
