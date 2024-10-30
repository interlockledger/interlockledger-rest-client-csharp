// ******************************************************************************************************************************
//
// Copyright (c) 2018-2022 InterlockLedger Network
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met
//
// * Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
// * Neither the name of the copyright holder nor the names of its
//   contributors may be used to endorse or promote products derived from
//   this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES, LOSS OF USE, DATA, OR PROFITS, OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//
// ******************************************************************************************************************************

using System.Diagnostics;
using InterlockLedger.Rest.Client.V14_2_2;

namespace InterlockLedger.Rest.Client.Abstractions;

[DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
public abstract class RestAbstractChain<T> : IRestChain where T : IRestChain
{
    public string Id { get; }
    public IInterlockings Interlockings { get; }
    public string? Name { get; }

    public IRecordsStore Records { get; }

    public IRecordsAsJsonStore RecordsAsJson { get; }

    public Task<IEnumerable<ulong>?> GetActiveAppsAsync() => _node.GetAsync<IEnumerable<ulong>>($"/chain/{Id}/activeApps");

    public Task<IEnumerable<KeyModel>?> GetPermittedKeysAsync() => _node.GetAsync<IEnumerable<KeyModel>>($"/chain/{Id}/key");

    public Task<ChainSummaryModel?> GetSummaryAsync() => _node.GetAsync<ChainSummaryModel>($"/chain/{Id}");

    public Task<IEnumerable<ulong>?> PermitAppsAsync(params ulong[] appsToPermit)
        => _node.PostAsync<IEnumerable<ulong>>($"/chain/{Id}/activeApps", appsToPermit);

    public Task<IEnumerable<KeyModel>?> PermitKeysAsync(params KeyPermitModel[] keysToPermit)
        => _node.PostAsync<IEnumerable<KeyModel>>($"/chain/{Id}/key", keysToPermit);

    public Task<PageOf<RecordModel>?> RecordsFromAsync(ulong firstSerial)
        => _node.GetAsync<PageOf<RecordModel>>($"records@{Id}?firstSerial={firstSerial}");

    public override string ToString() => $"Chain '{Name}' #{Id}";

    internal readonly RestAbstractNode<T> _node;
    internal async Task<bool> CanWriteForAppAsync(ulong appId) {
        var summary = await GetSummaryAsync().ConfigureAwait(false);
        if (summary is null || summary.IsClosedForNewTransactions || summary.ActiveApps.None(a => a == appId) || summary.LicensedApps.None(a => a == appId)) return false;
        var keys = await GetPermittedKeysAsync().ConfigureAwait(false);
        return keys.Safe().FirstOrDefault(k => k.Id == _node.CertificateKeyId) != null;
    }
    internal async Task<bool> MayHaveAppAsync(ulong appId) {
        var page = await Records.RecordsForAppFromAsync(appId, pageSize:1, ommitPayload:true);
        return page is not null && page.Items.Any();
    }

    protected RestAbstractChain(RestAbstractNode<T> node, ChainIdModel chainId) {
        _node = node.Required();
        Id = chainId.Required().Id.Required();
        Name = chainId.Name;
        Records = new RecordsStoreImplementation<T>(this);
        RecordsAsJson = new RecordsAsJsonStoreImplementation<T>(this);
        Interlockings = new InterlockingsImplementation<T>(this);
    }

    private string GetDebuggerDisplay() => $"{this} at {_node}";
}