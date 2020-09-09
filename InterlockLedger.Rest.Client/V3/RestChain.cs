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
using System.IO;
using InterlockLedger.Rest.Client.Abstractions;

namespace InterlockLedger.Rest.Client.V3
{
    public interface IDocumentApp
    {
        IEnumerable<DocumentDetailsModel> Documents { get; }

        string DocumentAsPlain(string fileId);

        RawDocumentModel DocumentAsRaw(string fileId);

        DocumentDetailsModel StoreDocumentFromBytes(byte[] bytes, DocumentUploadModel model);

        DocumentDetailsModel StoreDocumentFromBytes(byte[] bytes, string name, string contentType);

        DocumentDetailsModel StoreDocumentFromFile(string filePath, string name, string contentType)
            => StoreDocumentFromFile(filePath, new DocumentUploadModel(name, contentType));

        DocumentDetailsModel StoreDocumentFromFile(string filePath, DocumentUploadModel model);

        DocumentDetailsModel StoreDocumentFromText(string content, string name, string contentType = "plain/text")
            => StoreDocumentFromBytes(content.UTF8Bytes(), name, contentType);
    }

    public class RestChain : RestAbstractChain, IDocumentApp
    {
        IEnumerable<DocumentDetailsModel> IDocumentApp.Documents
            => _rest.Get<IEnumerable<DocumentDetailsModel>>($"/documents@{Id}");

        string IDocumentApp.DocumentAsPlain(string fileId)
            => _rest.CallApiPlainDoc($"/documents@{Id}/{fileId}", "GET");

        RawDocumentModel IDocumentApp.DocumentAsRaw(string fileId)
            => _rest.CallApiRawDoc($"/documents@{Id}/{fileId}", "GET");

        DocumentDetailsModel IDocumentApp.StoreDocumentFromBytes(byte[] bytes, string name, string contentType)
           => PostDocument(bytes, new DocumentUploadModel(name, contentType));

        DocumentDetailsModel IDocumentApp.StoreDocumentFromBytes(byte[] bytes, DocumentUploadModel model)
           => PostDocument(bytes, model);

        DocumentDetailsModel IDocumentApp.StoreDocumentFromFile(string filePath, DocumentUploadModel model) {
            if (!File.Exists(filePath))
                throw new ArgumentException($"No file '{filePath}' to store as a document!");
            if (model.Name == "?")
                model.Name = Path.GetFileName(filePath);
            return PostDocument(File.ReadAllBytes(filePath), model);
        }

        internal RestChain(IRestNodeInternals rest, ChainIdModel chainId) : base(rest, chainId) {
        }
    }
}