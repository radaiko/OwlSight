using System.Text.Json.Serialization;

namespace OwlSight.Core.Review;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ReviewSeverity
{
    Nitpick = 0,
    Info = 1,
    Warning = 2,
    Critical = 3
}
