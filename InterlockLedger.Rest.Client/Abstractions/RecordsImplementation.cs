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

namespace InterlockLedger.Rest.Client.Abstractions;

internal sealed class RecordsImplementation : IRestRecords
{
    public RecordsImplementation(RestAbstractChain parent) {
        _parent = parent.Required(nameof(parent));
        _rest = _parent._rest;
        _id = _parent.Id;
    }

    public Task<RecordModel> AddRecordAsync(NewRecordModel model)
        => _rest.PostAsync<RecordModel>($"records@{_id}", model);

    public Task<RecordModel> AddRecordAsync(ulong applicationId, ulong payloadTagId, byte[] bytes)
        => AddRecordAsync(applicationId, payloadTagId, RecordType.Data, bytes);

    public Task<RecordModel> AddRecordAsync(ulong applicationId, ulong payloadTagId, RecordType type, byte[] bytes)
        => _rest.PostRawAsync<RecordModel>($"records@{_id}/with?applicationId={applicationId}&payloadTagId={payloadTagId}&type={type}", bytes, "application/interlockledger");

    public Task<PageOf<RecordModel>> RecordsFromAsync(ulong firstSerial, ushort page = 0, byte pageSize = 10)
        => _rest.GetAsync<PageOf<RecordModel>>($"records@{_id}?firstSerial={firstSerial}&page={page}&pageSize={pageSize}");

    public Task<PageOf<RecordModel>> RecordsFromToAsync(ulong firstSerial, ulong lastSerial, ushort page = 0, byte pageSize = 10)
        => _rest.GetAsync<PageOf<RecordModel>>($"records@{_id}?firstSerial={firstSerial}&lastSerial={lastSerial}&page={page}&pageSize={pageSize}");

    private readonly string _id;
    private readonly RestAbstractChain _parent;
    private readonly IRestNodeInternals _rest;
}