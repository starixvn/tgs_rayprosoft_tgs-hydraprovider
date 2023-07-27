using System.Diagnostics;
using System.Net;
using Yarp.ReverseProxy.Forwarder;

namespace Tgs.HydraProvider;

internal class LoginPageConfiguration
{
	public HttpMessageInvoker HttpClient { get; } = new HttpMessageInvoker(new SocketsHttpHandler
	{
		UseProxy = false,
		AllowAutoRedirect = false,
		AutomaticDecompression = DecompressionMethods.None,
		UseCookies = false,
		ActivityHeadersPropagator = new ReverseProxyPropagator(DistributedContextPropagator.Current),
		ConnectTimeout = TimeSpan.FromSeconds(15),
	});

	public ForwarderRequestConfig RequestOptions { get; } = new ForwarderRequestConfig
	{
		ActivityTimeout = TimeSpan.FromSeconds(100)
	};

	public LoginPageTransformer Transformer { get; }

	public LoginPageConfiguration(LoginPageTransformer loginPageTransformer)
	{
		Transformer = loginPageTransformer ?? throw new ArgumentNullException(nameof(loginPageTransformer));
	}
}
