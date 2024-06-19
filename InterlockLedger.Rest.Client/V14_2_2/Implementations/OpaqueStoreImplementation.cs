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


namespace InterlockLedger.Rest.Client.V14_2_2;

internal sealed class OpaqueStoreImplementation : IOpaqueStore
{
    public OpaqueStoreImplementation(RestAbstractNode<RestChainV14_2_2> node, RestChainV14_2_2 chain) {
        _chain = chain.Required();
        _node = node;
        _id = _chain.Id;
    }

    public Task<OpaqueRecordModel?> AddRecordAsync(ulong appId, ulong payloadTypeId, ulong lastChangedRecordSerial, Stream source)
        => _node.PostStreamAsync<OpaqueRecordModel>($"/opaque/{_id}?appId={appId}&payloadTypeId={payloadTypeId}&lastChangedRecordSerial={lastChangedRecordSerial}", source.Required(), contentType: "application/octet-stream");

    public Task<OpaqueRecordModel?> AddRecordAsync(ulong appId, ulong payloadTypeId, ulong lastChangedRecordSerial, byte[] bytes) {
        using var body = new MemoryStream(bytes);
        return AddRecordAsync(appId, payloadTypeId, lastChangedRecordSerial, body);
    }
    public Task<(ulong AppId, ulong PayloadTypeId, DateTimeOffset? CreatedAt, Stream Content)?> RetrieveSinglePayloadAsync(ulong serial)
        => _node.GetOpaqueStreamAsync($"/opaque/{_id}@{serial}");

    public Task<PageOfOpaqueRecordsModel?> QueryRecordsFromAsync(ulong appId, ushort page = 0, byte pageSize = 10, bool lastToFirst = false, ulong[]? payloadTypeIds = null, ulong? howMany = null) {
        string url = $"/opaque/{_id}/asJson/query?appId={appId}&page={page}&pageSize={pageSize}&lastToFirst={lastToFirst}";
        if (howMany != null) {
            url += $"&howMany={howMany}";
        }
        if (payloadTypeIds != null) {
            foreach (var payloadTypeId in payloadTypeIds) {
                url += $"&payloadTypeIds={payloadTypeId}";
            }
        }
        return _node.GetAsync<PageOfOpaqueRecordsModel>(url);
    }


    private readonly string _id;
    private readonly RestChainV14_2_2 _chain;
    private readonly RestAbstractNode<RestChainV14_2_2> _node;
}
