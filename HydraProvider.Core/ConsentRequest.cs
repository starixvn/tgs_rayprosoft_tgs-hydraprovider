using System.Text.Json.Serialization;

namespace Tgs.HydraProvider;

public record ConsentRequest(
	string Challenge,
	[property: JsonPropertyName("requested_scope")] string[] RequestedScopes,
	bool Skip,
	string Subject,
	OAuthClient Client,
	LoginContext? Context,
	[property: JsonPropertyName("request_url")] Uri RequestUri,
	[property: JsonPropertyName("session_id")] string SessionId)
{
	public TokenSession CreateTokenSession()
		=> Context is null
			? new TokenSession(null, null)
			: new TokenSession(
				new Dictionary<string, object?>
				{
					["account_id"] = Context.AccountId,
					["brand_id"] = Context.BrandId,
					["system"] = Context.System
				},
				new Dictionary<string, object?>
				{
					["account_id"] = Context.AccountId,
				});
}
