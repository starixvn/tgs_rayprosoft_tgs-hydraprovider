namespace Tgs.HydraProvider.ApplicationServices;
public record CreateAccountCommand(
	string System,
	Guid BrandId,
	string UserName,
	string Password);
