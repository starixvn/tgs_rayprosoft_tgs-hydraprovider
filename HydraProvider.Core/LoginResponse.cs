using System.Text.Json.Serialization;

namespace Tgs.HydraProvider;

public record LoginResponse(
	[property: JsonPropertyName("redirect_to")] Uri RedirectTo);
