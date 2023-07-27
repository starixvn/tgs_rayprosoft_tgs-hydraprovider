using System.Text;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Yarp.ReverseProxy.Forwarder;
using Yarp.ReverseProxy.Transforms;

namespace Tgs.HydraProvider;

internal class LoginPageTransformer : HttpTransformer
{
	private readonly ILogger<LoginPageTransformer> _logger;

	public LoginPageTransformer(ILogger<LoginPageTransformer> logger)
	{
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

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

		proxyRequest.Headers.Add("X-Tunnel-Authorization", "tunnel eyJhbGciOiJFUzI1NiIsImtpZCI6IjNGMDM3NTk3MDlCNUI2REE1ODU3MkIyNjRENjE2QUZGQTQxMzA0MjciLCJ0eXAiOiJKV1QifQ.eyJjbHVzdGVySWQiOiJhc3NlIiwidHVubmVsSWQiOiJ6OXAwd3F2eiIsInNjcCI6ImNvbm5lY3QiLCJleHAiOjE2OTA1MjIyMTYsImlzcyI6Imh0dHBzOi8vdHVubmVscy5hcGkudmlzdWFsc3R1ZGlvLmNvbS8iLCJuYmYiOjE2OTA0MzQ5MTZ9.1XGAkZx09PIxwW9uruYZHiwejNTaC2jT2e9kgnnIOq2uKs4z9cfEvfTIR - sWOXiR04P_zhvJnPRrmomqnqKoyA");

		proxyRequest.RequestUri = RequestUtilities.MakeDestinationAddress(
			destinationPrefix,
			"/login.html",
			QueryString.Empty);

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

			_logger.LogInformation("{html}", htmlString);

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
