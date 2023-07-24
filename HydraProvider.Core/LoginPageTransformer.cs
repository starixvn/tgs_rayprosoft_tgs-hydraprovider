using System.Text;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Http;
using Yarp.ReverseProxy.Forwarder;
using Yarp.ReverseProxy.Transforms;

namespace Tgs.HydraProvider;

internal class LoginPageTransformer : HttpTransformer
{
	public override async ValueTask TransformRequestAsync(
		HttpContext httpContext,
		HttpRequestMessage proxyRequest,
		string destinationPrefix,
		CancellationToken cancellationToken)
	{
		await base.TransformRequestAsync(
			httpContext,
			proxyRequest,
			destinationPrefix,
			cancellationToken).ConfigureAwait(false);

		var queryContext = new QueryTransformContext(httpContext.Request);

		proxyRequest.RequestUri = RequestUtilities.MakeDestinationAddress(
			destinationPrefix,
			"/login.html",
			queryContext.QueryString);

		proxyRequest.Headers.Host = null;
	}

	public override async ValueTask<bool> TransformResponseAsync(
		HttpContext httpContext,
		HttpResponseMessage? proxyResponse,
		CancellationToken cancellationToken)
	{
		if (proxyResponse is not null)
		{
			var originContent = proxyResponse.Content;

			var htmlString = await proxyResponse.Content
				.ReadAsStringAsync(cancellationToken)
				.ConfigureAwait(false);

			if (!string.IsNullOrEmpty(htmlString))
			{
				var htmlDoc = new HtmlDocument();
				htmlDoc.LoadHtml(htmlString);

				var baseNode = htmlDoc.DocumentNode.SelectSingleNode("//form[@id='fLogin']");
				baseNode.SetAttributeValue("method", "post");
				baseNode.SetAttributeValue("action", $"{httpContext.Request.Path}{httpContext.Request.QueryString}");

				proxyResponse.Content = new StringContent(
					htmlDoc.DocumentNode.OuterHtml, Encoding.UTF8, originContent.Headers.ContentType!);
			}
		}

		return await base.TransformResponseAsync(
			httpContext,
			proxyResponse,
			cancellationToken).ConfigureAwait(false);
	}
}
