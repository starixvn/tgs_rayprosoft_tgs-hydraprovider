using System.Text.Json;
using Json.Patch;
using Json.Pointer;

namespace Tgs.HydraProvider;
public record AccountSnapshot(
	Guid Id,
	string UserName,
	Guid BrandId,
	string System,
	string Algorithm,
	string PasswordHash,
	DateTime CreatedOn)
{
	public AccountEvent CreateChangeEvent(Guid eventId, AccountSnapshot? baseSnapshot, DateTime createdOn)
	{
		var changeSets = baseSnapshot is null
			? new JsonPatch(PatchOperation.Add(
				JsonPointer.Create<AccountSnapshot>(o => o),
				JsonSerializer.SerializeToNode(this, HydraProviderUtility.SerializerOptions)))
			: baseSnapshot.CreatePatch(this, HydraProviderUtility.SerializerOptions);

		return new AccountEvent(
			eventId,
			Id,
			JsonSerializer.SerializeToDocument(changeSets),
			createdOn);
	}
}
