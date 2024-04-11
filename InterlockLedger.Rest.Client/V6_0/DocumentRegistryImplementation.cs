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
#nullable enable

namespace InterlockLedger.Rest.Client.V6_0;

internal class DocumentRegistryImplementation<ChainType>(RestAbstractNode<ChainType> node) : IDocumentRegistry where ChainType : IRestChain
{
    private readonly RestAbstractNode<ChainType> _node = node.Required();

    Uri IDocumentRegistry.BaseUri => _node.BaseUri;

    Task<DocumentsUploadConfiguration> IDocumentRegistry.GetDocumentsUploadConfigurationAsync()
            => _node.GetAsync<DocumentsUploadConfiguration>("/documents/configuration");

    public async Task<IEnumerable<string>> ListKnownChainsAcceptingTransactionsAsync() {
        var result = new List<string>();
        foreach (IRestChain chain in (await _node.GetChainsAsync().ConfigureAwait(false)).Safe()) {
            var summary = await chain.GetSummaryAsync().ConfigureAwait(false);
            if (summary is not null
                && !summary.IsClosedForNewTransactions
                && !summary.LicenseIsExpired
                && summary.LicensedApps.Contains(4ul)
                && summary.ActiveApps.Contains(4ul))
                result.Add(chain.Id);
        }
        return result;
    }

    public async Task<IEnumerable<string>> ListKnownChainsAsync()
        => (await _node.GetChainsAsync().ConfigureAwait(false)).Safe().Select(ch => ch.Id);

    Task<DocumentsMetadataModel> IDocumentRegistry.RetrieveMetadataAsync(string locator)
        => _node.GetAsync<DocumentsMetadataModel>(FromLocator(locator.Required(), "metadata"));

    Task<(string Name, string ContentType, Stream Content)> IDocumentRegistry.RetrieveSingleAsync(string locator, int index)
        => _node.GetFileReadStreamAsync(FromLocator(locator.Required(), index));

    Task<(string Name, string ContentType, Stream Content)> IDocumentRegistry.RetrieveZipAsync(string locator)
        => _node.GetFileReadStreamAsync(FromLocator(locator.Required(), "zip"), accept: "application/zip");

    Task<DocumentsTransactionModel> IDocumentRegistry.TransactionAddItemAsync(string transactionId, string path, string name, string comment, string contentType, Stream source)
        => _node.PostStreamAsync<DocumentsTransactionModel>($"/documents/transaction/{transactionId.Required()}?name={HttpUtility.UrlEncode(name.Required())}&comment={HttpUtility.UrlEncode(comment)}", source.Required(), contentType.Required());

    async Task<DocumentsTransactionModel> IDocumentRegistry.TransactionBeginAsync(DocumentsBeginTransactionModel transactionStart) {
        await ValidateChainAsync(transactionStart.Required().Chain);
        return await _node.PostAsync<DocumentsTransactionModel>("/documents/transaction", transactionStart);
    }

    Task<string> IDocumentRegistry.TransactionCommitAsync(string transactionId)
        => _node.PostAsync<string>($"/documents/transaction/{transactionId.Required()}/commit", null);

    Task<DocumentsTransactionModel> IDocumentRegistry.TransactionStatusAsync(string transactionId)
        => _node.GetAsync<DocumentsTransactionModel>($"/documents/transaction/{transactionId.Required()}");
   
    private static string FromLocator<T>(string locator, T selector) => $"/documents/{HttpUtility.UrlEncode(locator)}/{selector}";

    private async Task ValidateChainAsync(string chain) {
        if ((await ListKnownChainsAsync()).None(id => id == chain))
            throw new ArgumentException($"Chain '{chain}' is unknown");
        if ((await ListKnownChainsAcceptingTransactionsAsync()).None(id => id == chain))
            throw new ArgumentException($"Chain '{chain}' can't accept document transactions");
    }
}