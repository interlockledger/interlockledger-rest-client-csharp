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
using System.Threading.Tasks;

namespace InterlockLedger.Rest.Client.V6_0
{
    public interface IDocumentsApp
    {
        Uri BaseUri { get; }

        Task<DocumentsUploadConfiguration> GetDocumentsUploadConfigurationAsync();

        Task<IEnumerable<string>> ListKnownChainsAcceptingTransactionsAsync();

        Task<IEnumerable<string>> ListKnownChainsAsync();

        Task<DocumentsMetadataModel> RetrieveMetadataAsync(string locator);

        Task<(string Name, string ContentType, Stream Content)> RetrieveSingleAsync(string locator, int index);

        Task<(string Name, string ContentType, Stream Content)> RetrieveZipAsync(string locator);

        Task<DocumentsTransactionModel> TransactionAddItemAsync(string transactionId, string path, string name, string comment, string contentType, Stream source);

        Task<DocumentsTransactionModel> TransactionAddItemAsync(string transactionId, DirectoryInfo baseDirectory, FileInfo file, string comment, string contentType)
            => string.IsNullOrWhiteSpace(transactionId)
                ? throw new ArgumentNullException(nameof(transactionId))
                : baseDirectory is null
                    ? throw new ArgumentNullException(nameof(baseDirectory))
                    : file is null
                        ? throw new ArgumentNullException(nameof(file))
                        : string.IsNullOrWhiteSpace(contentType)
                            ? throw new ArgumentNullException(nameof(contentType))
                            : !file.FullName.StartsWith(baseDirectory.FullName, StringComparison.InvariantCultureIgnoreCase)
                                ? throw new ArgumentException($"File ´{file.FullName}´ is not inside directory '{baseDirectory.FullName}' ")
                                : TransactionAddItemAsync(transactionId,
                                                     Path.GetRelativePath(baseDirectory.FullName, file.DirectoryName),
                                                     file.Name,
                                                     comment,
                                                     contentType,
                                                     file.OpenRead());

        Task<DocumentsTransactionModel> TransactionBeginAsync(DocumentsBeginTransactionModel transactionStart);

        Task<string> TransactionCommitAsync(string transactionId);

        Task<DocumentsTransactionModel> TransactionStatusAsync(string transactionId);
    }
}