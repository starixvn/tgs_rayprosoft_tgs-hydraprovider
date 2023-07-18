using System.Text.Json.Serialization;

namespace Tgs.HydraProvider;

public record ConsentResponse(
	[property: JsonPropertyName("redirect_to")] Uri RedirectTo);
