using JK.Orleans.State;

namespace JK.Orleans.Grains;

public interface IApiMessageGrain: IGrainWithStringKey
{
    Task SendApiMessage(string fullUrl);
    Task<bool> Register(string cronString);
}