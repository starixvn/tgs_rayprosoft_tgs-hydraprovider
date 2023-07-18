namespace Tgs.HydraProvider;

public record LoginContext(
	Guid BrandId,
	string System,
	Guid AccountId);
