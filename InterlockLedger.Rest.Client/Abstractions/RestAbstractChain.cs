// ******************************************************************************************************************************
//
// Copyright (c) 2018-2021 InterlockLedger Network
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

namespace InterlockLedger.Rest.Client.Abstractions;

[DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
public abstract class RestAbstractChain : IRestChain
{
    public string Id { get; }
    IRestInterlockings IRestChain.Interlockings => _interlockings;
    public string Name { get; }

    IRestRecords IRestChain.Records => _records;

    IRestRecordsAsJson IRestChain.RecordsAsJson => _recordsAsJson;

    Task<IEnumerable<ulong>> IRestChain.GetActiveAppsAsync() => _rest.GetAsync<IEnumerable<ulong>>($"/chain/{Id}/activeApps");

    Task<IEnumerable<KeyModel>> IRestChain.GetPermittedKeysAsync() => _rest.GetAsync<IEnumerable<KeyModel>>($"/chain/{Id}/key");

    Task<ChainSummaryModel> IRestChain.GetSummaryAsync() => _rest.GetAsync<ChainSummaryModel>($"/chain/{Id}");

    Task<IEnumerable<ulong>> IRestChain.PermitAppsAsync(params ulong[] appsToPermit)
        => _rest.PostAsync<IEnumerable<ulong>>($"/chain/{Id}/activeApps", appsToPermit);

    Task<IEnumerable<KeyModel>> IRestChain.PermitKeysAsync(params KeyPermitModel[] keysToPermit)
        => _rest.PostAsync<IEnumerable<KeyModel>>($"/chain/{Id}/key", keysToPermit);

    public Task<PageOf<RecordModel>> RecordsFromAsync(ulong firstSerial)
        => _rest.GetAsync<PageOf<RecordModel>>($"records@{Id}?firstSerial={firstSerial}");

    public override string ToString() => $"Chain '{Name}' #{Id}";

    internal readonly IRestNodeInternals _rest;

    internal RestAbstractChain(IRestNodeInternals rest, ChainIdModel chainId) {
        if (chainId == null)
            throw new ArgumentNullException(nameof(chainId));
        _rest = rest ?? throw new ArgumentNullException(nameof(rest));
        Id = chainId.Id;
        Name = chainId.Name;
        _records = new RecordsImplementation(this);
        _recordsAsJson = new RecordsAsJsonImplementation(this);
        _interlockings = new InterlockingsImplementation(this);
    }

    private readonly IRestInterlockings _interlockings;
    private readonly IRestRecords _records;
    private readonly IRestRecordsAsJson _recordsAsJson;

    private string GetDebuggerDisplay() => $"{this} at {_rest}";
}
