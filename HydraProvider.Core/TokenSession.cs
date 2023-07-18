using System.Text.Json.Serialization;

namespace Tgs.HydraProvider;

public record TokenSession(
	[property: JsonPropertyName("access_token")] IReadOnlyDictionary<string, object?>? AccessToken,
	[property: JsonPropertyName("id_token")] IReadOnlyDictionary<string, object?>? IdToken);
