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
using System.Text;
using Newtonsoft.Json;

namespace InterlockLedger.Rest.Client
{
    public class DocumentBaseModel
    {
        /// <summary>
        /// Cipher algorithm used to cipher the document
        /// </summary>
        public CipherAlgorithms Cipher { get; set; }

        [JsonIgnore]
        public bool IsCiphered => Cipher != CipherAlgorithms.None && !string.IsNullOrWhiteSpace(KeyId);

        /// <summary>
        /// Unique id of key that ciphers this
        /// </summary>
        public string KeyId { get; set; }

        /// <summary>
        /// Document name (may be a file name with an extension)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// A reference to a previous version of this document (ChainId and RecordNumber)
        /// </summary>
        public string PreviousVersion { get; set; }
    }

    public class DocumentDetailsModel : DocumentBaseModel
    {
        /// <summary>
        /// Document content type (mime-type)
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// Unique id of the document
        /// -- derived from its content, so the same content stored in different chains
        /// -- will have the same FileId
        /// </summary>
        public string FileId { get; set; }

        [JsonIgnore]
        public bool IsPlainText => ContentType == "plain/text";

        /// <summary>
        /// Compound id for this document as stored in this chain
        /// </summary>
        public string PhysicalDocumentID { get; set; }

        public override string ToString() => $"Document '{Name}' [{ContentType}] {FileId}";
    }

    public class DocumentUploadModel : DocumentBaseModel
    {
        public DocumentUploadModel(string name, string contentType) {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Document must have a Name", nameof(name));
            if (string.IsNullOrWhiteSpace(contentType))
                throw new ArgumentException("Document must have a ContentType", nameof(contentType));
            Name = name;
            ContentType = contentType;
        }

        /// <summary>
        /// Document content type (mime-type)
        /// </summary>
        public string ContentType { get; }

        public string ToQueryString() {
            var sb = new StringBuilder($"?cipher={Cipher}&name={Name}");
            if (!string.IsNullOrWhiteSpace(KeyId))
                sb.Append("&keyId=").Append(KeyId);
            if (!string.IsNullOrWhiteSpace(PreviousVersion))
                sb.Append("&previousVersion=").Append(PreviousVersion);
            return sb.ToString();
        }
    }
}