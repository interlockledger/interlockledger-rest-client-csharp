using InterlockLedger.Rest.Client.Abstractions;

namespace InterlockLedger.Rest.Client.V4_3
{
    public class RestChain : RestAbstractChain
    {
        internal RestChain(RestNode rest, ChainIdModel chainId) : base(rest, chainId) {
        }
    }
}