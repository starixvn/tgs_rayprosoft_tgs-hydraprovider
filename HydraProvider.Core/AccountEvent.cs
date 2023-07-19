using System.Text.Json;

namespace Tgs.HydraProvider;
public record AccountEvent(
	Guid Id,
	Guid AccountId,
	JsonDocument ChangeSets,
	DateTime CreatedOn);
