/******************************************************************************************************************************

Copyright (c) 2018-2020 InterlockLedger Network
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this
  list of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice,
  this list of conditions and the following disclaimer in the documentation
  and/or other materials provided with the distribution.

* Neither the name of the copyright holder nor the names of its
  contributors may be used to endorse or promote products derived from
  this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

******************************************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InterlockLedger.Rest.Client.V3;

namespace InterlockLedger.Rest.Client.Abstractions
{
    public abstract class RestAbstractChain
    {
        internal readonly IRestNodeInternals _rest;

        internal RestAbstractChain(IRestNodeInternals rest, ChainIdModel chainId) {
            if (chainId == null)
                throw new ArgumentNullException(nameof(chainId));
            _rest = rest ?? throw new ArgumentNullException(nameof(rest));
            Id = chainId.Id;
            Name = chainId.Name;
        }

        public Task<IEnumerable<ulong>> GetActiveAppsAsync() => _rest.GetAsync<IEnumerable<ulong>>($"/chain/{Id}/activeApps");
        public string Id { get; }

        public Task<IEnumerable<InterlockingRecordModel>> GetInterlocksAsync() => _rest.GetAsync<IEnumerable<InterlockingRecordModel>>($"/chain/{Id}/interlockings");
        public string Name { get; }

        public Task<IEnumerable<KeyModel>> GetPermittedKeysAsync() => _rest.GetAsync<IEnumerable<KeyModel>>($"/chain/{Id}/key");
        public Task<ChainSummaryModel> GetSummaryAsync() => _rest.GetAsync<ChainSummaryModel>($"/chain/{Id}");

        public Task<RecordModel> AddRecordAsync(NewRecordModel model)
            => _rest.PostAsync<RecordModel>($"records@{Id}", model);

        public Task<RecordModel> AddRecordAsync(ulong applicationId, ulong payloadTagId, byte[] bytes)
            => AddRecordAsync(applicationId, payloadTagId, RecordType.Data, bytes);

        public Task<RecordModel> AddRecordAsync(ulong applicationId, ulong payloadTagId, RecordType type, byte[] bytes)
            => _rest.PostRawAsync<RecordModel>($"records@{Id}/with?applicationId={applicationId}&payloadTagId={payloadTagId}&type={type}", bytes, "application/interlockledger");

        public Task<RecordModelAsJson> AddRecordAsJsonAsync(NewRecordModelAsJson model)
            => AddRecordAsJsonAsync(model.ApplicationId, model.PayloadTagId, model.Type, model.Json);

        public Task<RecordModelAsJson> AddRecordAsJsonAsync(ulong applicationId, ulong payloadTagId, object payload)
            => AddRecordAsJsonAsync(applicationId, payloadTagId, RecordType.Data, payload);

        public Task<RecordModelAsJson> AddRecordAsJsonAsync(ulong applicationId, ulong payloadTagId, RecordType type, object payload)
            => _rest.PostAsync<RecordModelAsJson>($"records@{Id}/asJson?applicationId={applicationId}&payloadTagId={payloadTagId}&type={type}", payload);

        public Task<InterlockingRecordModel> ForceInterlockAsync(ForceInterlockModel model)
            => _rest.PostAsync<InterlockingRecordModel>($"/chain/{Id}/interlockings", model);

        public Task<IEnumerable<ulong>> PermitAppsAsync(params ulong[] appsToPermit)
            => _rest.PostAsync<IEnumerable<ulong>>($"/chain/{Id}/activeApps", appsToPermit);

        public Task<IEnumerable<KeyModel>> PermitKeysAsync(params KeyPermitModel[] keysToPermit)
            => _rest.PostAsync<IEnumerable<KeyModel>>($"/chain/{Id}/key", keysToPermit);

        public Task<IEnumerable<RecordModel>> RecordsFromAsync(ulong firstSerial)
            => _rest.GetAsync<IEnumerable<RecordModel>>($"records@{Id}?firstSerial={firstSerial}");

        public Task<IEnumerable<RecordModelAsJson>> RecordsFromAsJsonAsync(ulong firstSerial)
            => _rest.GetAsync<IEnumerable<RecordModelAsJson>>($"records@{Id}/asJson?firstSerial={firstSerial}");

        public Task<IEnumerable<RecordModel>> RecordsFromToAsync(ulong firstSerial, ulong lastSerial)
            => _rest.GetAsync<IEnumerable<RecordModel>>($"records@{Id}?firstSerial={firstSerial}&lastSerial={lastSerial}");

        public Task<IEnumerable<RecordModelAsJson>> RecordsFromToAsJsonAsync(ulong firstSerial, ulong lastSerial)
            => _rest.GetAsync<IEnumerable<RecordModelAsJson>>($"records@{Id}/asJson?firstSerial={firstSerial}&lastSerial={lastSerial}");

        public override string ToString() => $"Chain '{Name}' #{Id}";

    }
}