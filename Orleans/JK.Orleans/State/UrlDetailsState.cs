namespace JK.Orleans.State;

[GenerateSerializer, Alias(nameof(UrlDetailsState))]
public sealed record class UrlDetailsState
{
    [Id(0)] public string FullUrl { get; set; } = null!;

    [Id(1)] public string ShortenedRouteSegment { get; set; } = null!;
}
