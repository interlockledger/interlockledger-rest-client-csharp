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

public interface IDocumentRegistry
{
    Task<DocumentsUploadConfiguration?> GetDocumentsUploadConfigurationAsync();

    Task<IEnumerable<string>?> ListKnownChainsAcceptingTransactionsAsync();

    Task<IEnumerable<string>?> ListKnownChainsAsync();

    Task<DocumentsMetadataModel?> RetrieveMetadataAsync(string locator);

    Task<(string Name, string ContentType, Stream Content)?> RetrieveSingleAsync(string locator, int index);

    Task<(string Name, string ContentType, Stream Content)?> RetrieveZipAsync(string locator, bool omitFromParentControlFile = false, bool omitToChildrenControlFile = false);

    Task<DocumentsTransactionModel?> TransactionAddItemAsync(string transactionId, string path, string name, string? comment, string contentType, Stream source);

    Task<DocumentsTransactionModel?> TransactionAddItemAsync(string transactionId, DirectoryInfo baseDirectory, FileInfo file, string? comment, string contentType)
        => !file.Required().FullName.StartsWith(baseDirectory.Required().FullName, StringComparison.InvariantCultureIgnoreCase)
            ? throw new ArgumentException(message: $"File ´{file.FullName}´ is not inside directory '{baseDirectory.FullName}' ", nameof(file))
            : TransactionAddItemAsync(transactionId.Required(),
                                      Path.GetRelativePath(baseDirectory.FullName, file.DirectoryName.Required()),
                                      file.Name,
                                      comment,
                                      contentType.Required(),
                                      file.OpenRead());

    Task<DocumentsTransactionModel?> TransactionBeginAsync(DocumentsBeginTransactionModel transactionStart);

    Task<string?> TransactionCommitAsync(string transactionId);

    Task<DocumentsTransactionModel?> TransactionStatusAsync(string transactionId);
}