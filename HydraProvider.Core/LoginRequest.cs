using System.Collections.Specialized;
using System.Text.Json.Serialization;
using System.Web;

namespace Tgs.HydraProvider;

public record LoginRequest(
	string Challenge,
	[property: JsonPropertyName("requested_scope")] string[] RequestedScopes,
	bool Skip,
	string Subject,
	OAuthClient Client,
	[property: JsonPropertyName("request_url")] Uri RequestUri,
	[property: JsonPropertyName("session_id")] string SessionId)
{
	public string? GetSystem()
	{
		var systemScope = Client.Scopes.Split(' ')
			.Where(scope => scope.StartsWith("system:"))
			.FirstOrDefault();

		return systemScope?.Split(':').Last();
	}

	public Guid? GetBrandId()
	{
		var queryDict = HttpUtility.ParseQueryString(RequestUri.Query) ?? new NameValueCollection();

		var brandId = queryDict.GetValues("brand_id")?.FirstOrDefault();

		return brandId is null
			? null
			: Guid.Parse(brandId);
	}
}
