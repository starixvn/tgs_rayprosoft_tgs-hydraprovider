using System.Text.Json.Serialization;
using Tgs.HydraProvider;

namespace Tgs.HydraProvider;

public record AcceptLoginRequest(
	string Subject,
	LoginContext? Context = null,
	bool? Remember = null,
	[property: JsonPropertyName("remember_for")] long? RememberFor = null);
