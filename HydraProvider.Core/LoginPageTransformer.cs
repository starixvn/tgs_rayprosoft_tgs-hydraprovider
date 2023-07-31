using System.Text;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Yarp.ReverseProxy.Forwarder;

namespace Tgs.HydraProvider;

internal class LoginPageTransformer : HttpTransformer
{
	private readonly IOptionsMonitor<HydraProviderOptions> _optionsMonitor;
	private readonly ILogger<LoginPageTransformer> _logger;

	public LoginPageTransformer(IOptionsMonitor<HydraProviderOptions> optionsMonitor, ILogger<LoginPageTransformer> logger)
	{
		_optionsMonitor = optionsMonitor ?? throw new ArgumentNullException(nameof(optionsMonitor));
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

		proxyRequest.Headers.Add("X-Tunnel-Authorization", $"tunnel {_optionsMonitor.CurrentValue.DevTunnelToken}");

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

			_logger.LogInformation(
				"{Headers}",
				string.Join("&", proxyResponse.Headers
					.Select(kv => $"{kv.Key}={string.Join(", ", kv.Value)}")));
			_logger.LogInformation("{Html}", htmlString);

			if (!string.IsNullOrEmpty(htmlString))
			{
				var htmlDoc = new HtmlDocument();
				htmlDoc.LoadHtml(htmlString);

				var baseNode = htmlDoc.DocumentNode.SelectSingleNode("//form[@id='fLogin']");
				baseNode?.SetAttributeValue("method", "post");
				baseNode?.SetAttributeValue("action", $"{httpContext.Request.Path}{httpContext.Request.QueryString}");

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
