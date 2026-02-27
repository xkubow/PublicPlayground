using JK.Orleans.State;

namespace JK.Orleans.Grains;

public sealed class UrlShortenerGrain( [PersistentState( stateName: "url", storageName: "orleans")] IPersistentState<UrlDetailsState> state)
    : Grain, IUrlShortenerGrain
{
    public async Task SetUrl(string fullUrl)
    {
        state.State = new()
        {
            ShortenedRouteSegment = this.GetPrimaryKeyString(),
            FullUrl = fullUrl
        };

        await state.WriteStateAsync();
    }

    public Task<string> GetUrl() => Task.FromResult(state.State.FullUrl);
}