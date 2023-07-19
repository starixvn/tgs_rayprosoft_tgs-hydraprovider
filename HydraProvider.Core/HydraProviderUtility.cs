using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tgs.HydraProvider;

internal static class HydraProviderUtility
{
	public static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
	{
		WriteIndented = false,
		Converters = {
			new JsonStringEnumConverter()
		},
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
	};
}
