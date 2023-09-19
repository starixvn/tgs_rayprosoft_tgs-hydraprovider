using System.Text.Json.Serialization;

namespace Tgs.HydraProvider;
public record RejectLoginRequest(
	[property: JsonPropertyName("status_code")] int StatusCode);
