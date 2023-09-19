namespace Tgs.HydraProvider.ViewModels;

public class LoginRequestViewModel
{
	public required Guid BrandId { get; set; }

	public required string UserName { get; set; }

	public required string Password { get; set; }
}
