using Microsoft.AspNetCore.Http;
using Yarp.ReverseProxy.Forwarder;

namespace Tgs.HydraProvider;

internal class LoginPageForwarder : ILoginPageForwarder
{
	private readonly LoginPageConfiguration _configuration;
	private readonly IHttpForwarder _httpForwarder;

	public LoginPageForwarder(LoginPageConfiguration configuration, IHttpForwarder httpForwarder)
	{
		_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
		_httpForwarder = httpForwarder ?? throw new ArgumentNullException(nameof(httpForwarder));
	}

	public ValueTask<ForwarderError> SendAsync(HttpContext context, string destinationPrefix, CancellationToken cancellationToken = default)
		=> _httpForwarder.SendAsync(
			context,
			destinationPrefix,
			_configuration.HttpClient,
			_configuration.RequestOptions,
			_configuration.Transformer,
			cancellationToken);
}
