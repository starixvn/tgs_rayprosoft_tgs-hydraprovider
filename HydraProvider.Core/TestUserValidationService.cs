using AspNetCore.Authentication.Basic;

namespace Tgs.HydraProvider;

internal class TestUserValidationService : IBasicUserValidationService
{
	public Task<bool> IsValidAsync(string username, string password)
		=> Task.FromResult(username == "test" && password == "123");
}
