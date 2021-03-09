using InterlockLedger.Rest.Client.Abstractions;

namespace InterlockLedger.Rest.Client.V6_0
{
    public class RestChain : RestAbstractChain, IRestChainV6_0
    {
        IJsonStore IRestChainV6_0.JsonStore => _jsonStore;

        internal RestChain(RestNode rest, ChainIdModel chainId) : base(rest, chainId) => _jsonStore = new JsonStoreImplementation(this);

        private readonly IJsonStore _jsonStore;
    }
}