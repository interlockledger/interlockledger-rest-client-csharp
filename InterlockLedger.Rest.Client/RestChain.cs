/******************************************************************************************************************************

Copyright (c) 2018-2019 InterlockLedger Network
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

namespace InterlockLedger.Rest.Client
{
    public class RestChain
    {
        public IEnumerable<ulong> ActiveApps => _rest.Get<IEnumerable<ulong>>($"/chain/{Id}/activeApps");
        public IEnumerable<DocumentDetailsModel> Documents => _rest.Get<IEnumerable<DocumentDetailsModel>>($"/chain/{Id}/document");
        public string Id { get; }
        public IEnumerable<InterlockingRecordModel> Interlocks => _rest.Get<IEnumerable<InterlockingRecordModel>>($"/chain/{Id}/interlock");
        public string Name { get; }
        public IEnumerable<KeyModel> PermittedKeys => _rest.Get<IEnumerable<KeyModel>>($"/chain/{Id}/key");
        public IEnumerable<RecordModel> Records => _rest.Get<IEnumerable<RecordModel>>($"/chain/{Id}/record");
        public ChainSummaryModel Summary => _rest.Get<ChainSummaryModel>($"/chain/{Id}");

        public RecordModel AddRecord(NewRecordModel model)
            => _rest.Post<RecordModel>($"/chain/{Id}/record", model);

        public RecordModel AddRecord(ulong applicationId, ulong payloadTagId, byte[] bytes)
            => AddRecord(applicationId, payloadTagId, RecordType.Data, bytes);

        public RecordModel AddRecord(ulong applicationId, ulong payloadTagId, RecordType type, byte[] bytes)
            => _rest.PostRaw<RecordModel>($"/chain/{Id}/record/with?applicationId={applicationId}&payloadTagId={payloadTagId}&type={type}", bytes, "application/interlockledger");

        public string DocumentAsPlain(string fileId)
            => _rest.CallApiPlainDoc($"/chain/{Id}/document/{fileId}", "GET");

        public RawDocumentModel DocumentAsRaw(string fileId)
            => _rest.CallApiRawDoc($"/chain/{Id}/document/{fileId}", "GET");

        public InterlockingRecordModel ForceInterlock(ForceInterlockModel model)
            => _rest.Post<InterlockingRecordModel>($"/chain/{Id}/interlock", model);

        public IEnumerable<ulong> PermitApps(params ulong[] appsToPermit)
            => _rest.Post<IEnumerable<ulong>>($"/chain/{Id}/activeApps", appsToPermit);

        public IEnumerable<KeyModel> PermitKeys(params KeyPermitModel[] keysToPermit)
            => _rest.Post<IEnumerable<KeyModel>>($"/chain/{Id}/key", keysToPermit);

        public IEnumerable<RecordModel> RecordsFrom(ulong firstSerial)
            => _rest.Get<IEnumerable<RecordModel>>($"/chain/{Id}/record?firstSerial={firstSerial}");

        public IEnumerable<RecordModel> RecordsFromTo(ulong firstSerial, ulong lastSerial)
            => _rest.Get<IEnumerable<RecordModel>>($"/chain/{Id}/record?firstSerial={firstSerial}&lastSerial={lastSerial}");

        public override string ToString() => $"Chain '{Name}' #{Id}";

        internal RestChain(RestNode rest, ChainIdModel chainId) {
            if (chainId == null)
                throw new ArgumentNullException(nameof(chainId));
            _rest = rest ?? throw new ArgumentNullException(nameof(rest));
            Id = chainId.Id;
            Name = chainId.Name;
        }

        private readonly RestNode _rest;
    }
}