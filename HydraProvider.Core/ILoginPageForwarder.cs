using Microsoft.AspNetCore.Http;
using Yarp.ReverseProxy.Forwarder;

namespace Tgs.HydraProvider;

public interface ILoginPageForwarder
{
	ValueTask<ForwarderError> SendAsync(HttpContext context, string destinationPrefix, CancellationToken cancellationToken = default);
}
