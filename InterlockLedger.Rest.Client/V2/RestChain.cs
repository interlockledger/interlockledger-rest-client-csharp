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
using System.IO;

namespace InterlockLedger.Rest.Client.V3
{
    public class RestChain
    {
        public IEnumerable<ulong> ActiveApps => _rest.Get<IEnumerable<ulong>>($"/chain/{Id}/activeApps");
        public IEnumerable<DocumentDetailsModel> Documents => _rest.Get<IEnumerable<DocumentDetailsModel>>($"/documents@{Id}");
        public string Id { get; }
        public IEnumerable<InterlockingRecordModel> Interlocks => _rest.Get<IEnumerable<InterlockingRecordModel>>($"/chain/{Id}/interlockings");
        public string Name { get; }
        public IEnumerable<KeyModel> PermittedKeys => _rest.Get<IEnumerable<KeyModel>>($"/chain/{Id}/key");
        public IEnumerable<RecordModel> Records => _rest.Get<IEnumerable<RecordModel>>($"records@{Id}");
        public IEnumerable<RecordModelAsJson> RecordsAsJson => _rest.Get<IEnumerable<RecordModelAsJson>>($"records@{Id}/asJson");

        public ChainSummaryModel Summary => _rest.Get<ChainSummaryModel>($"/chain/{Id}");

        public RecordModel AddRecord(NewRecordModel model)
            => _rest.Post<RecordModel>($"records@{Id}", model);

        public RecordModel AddRecord(ulong applicationId, ulong payloadTagId, byte[] bytes)
            => AddRecord(applicationId, payloadTagId, RecordType.Data, bytes);

        public RecordModel AddRecord(ulong applicationId, ulong payloadTagId, RecordType type, byte[] bytes)
            => _rest.PostRaw<RecordModel>($"records@{Id}/with?applicationId={applicationId}&payloadTagId={payloadTagId}&type={type}", bytes, "application/interlockledger");

        public RecordModelAsJson AddRecordAsJson(NewRecordModelAsJson model)
            => AddRecordAsJson(model.ApplicationId, model.PayloadTagId, model.Type, model.Json);

        public RecordModelAsJson AddRecordAsJson(ulong applicationId, ulong payloadTagId, object payload)
            => AddRecordAsJson(applicationId, payloadTagId, RecordType.Data, payload);

        public RecordModelAsJson AddRecordAsJson(ulong applicationId, ulong payloadTagId, RecordType type, object payload)
            => _rest.Post<RecordModelAsJson>($"records@{Id}/asJson?applicationId={applicationId}&payloadTagId={payloadTagId}&type={type}", payload);

        public string DocumentAsPlain(string fileId)
            => _rest.CallApiPlainDoc($"/documents@{Id}/{fileId}", "GET");

        public RawDocumentModel DocumentAsRaw(string fileId)
            => _rest.CallApiRawDoc($"/documents@{Id}/{fileId}", "GET");

        public InterlockingRecordModel ForceInterlock(ForceInterlockModel model)
            => _rest.Post<InterlockingRecordModel>($"/chain/{Id}/interlockings", model);

        public IEnumerable<ulong> PermitApps(params ulong[] appsToPermit)
            => _rest.Post<IEnumerable<ulong>>($"/chain/{Id}/activeApps", appsToPermit);

        public IEnumerable<KeyModel> PermitKeys(params KeyPermitModel[] keysToPermit)
            => _rest.Post<IEnumerable<KeyModel>>($"/chain/{Id}/key", keysToPermit);

        public IEnumerable<RecordModel> RecordsFrom(ulong firstSerial)
            => _rest.Get<IEnumerable<RecordModel>>($"records@{Id}?firstSerial={firstSerial}");

        public IEnumerable<RecordModelAsJson> RecordsFromAsJson(ulong firstSerial)
            => _rest.Get<IEnumerable<RecordModelAsJson>>($"records@{Id}/asJson?firstSerial={firstSerial}");

        public IEnumerable<RecordModel> RecordsFromTo(ulong firstSerial, ulong lastSerial)
            => _rest.Get<IEnumerable<RecordModel>>($"records@{Id}?firstSerial={firstSerial}&lastSerial={lastSerial}");

        public IEnumerable<RecordModelAsJson> RecordsFromToAsJson(ulong firstSerial, ulong lastSerial)
            => _rest.Get<IEnumerable<RecordModelAsJson>>($"records@{Id}/asJson?firstSerial={firstSerial}&lastSerial={lastSerial}");

        public DocumentDetailsModel StoreDocumentFromBytes(byte[] bytes, string name, string contentType)
            => PostDocument(bytes, new DocumentUploadModel(name, contentType));

        public DocumentDetailsModel StoreDocumentFromBytes(byte[] bytes, DocumentUploadModel model)
            => PostDocument(bytes, model);

        public DocumentDetailsModel StoreDocumentFromFile(string filePath, string name, string contentType)
            => StoreDocumentFromFile(filePath, new DocumentUploadModel(name, contentType));

        public DocumentDetailsModel StoreDocumentFromFile(string filePath, DocumentUploadModel model) {
            if (!File.Exists(filePath))
                throw new ArgumentException($"No file '{filePath}' to store as a document!");
            if (model.Name == "?")
                model.Name = Path.GetFileName(filePath);
            return PostDocument(File.ReadAllBytes(filePath), model);
        }

        public DocumentDetailsModel StoreDocumentFromText(string content, string name, string contentType = "plain/text")
            => StoreDocumentFromBytes(content.UTF8Bytes(), name, contentType);

        public override string ToString() => $"Chain '{Name}' #{Id}";

        internal RestChain(RestNode rest, ChainIdModel chainId) {
            if (chainId == null)
                throw new ArgumentNullException(nameof(chainId));
            _rest = rest ?? throw new ArgumentNullException(nameof(rest));
            Id = chainId.Id;
            Name = chainId.Name;
        }

        private readonly RestNode _rest;

        private DocumentDetailsModel PostDocument(byte[] bytes, DocumentUploadModel model) {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            return _rest.PostRaw<DocumentDetailsModel>($"/documents@{Id}{model.ToQueryString()}", bytes, model.ContentType);
        }
    }
}
