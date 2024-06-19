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


using System.Globalization;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace InterlockLedger.Rest.Client.V14_2_2;

internal class DocumentRegistryImplementation<ChainType>(RestAbstractNode<ChainType> node) : IDocumentRegistry where ChainType : IRestChain
{
    private const string _pathPrefix = "/documents";
    private const string _transactionPrefix = $"{_pathPrefix}/transaction";
    private readonly RestAbstractNode<ChainType> _node = node.Required();

    public Task<DocumentsUploadConfiguration?> GetDocumentsUploadConfigurationAsync()
            => _node.GetAsync<DocumentsUploadConfiguration>(UrlFromParts(_pathPrefix, "configuration"));

    public async Task<IEnumerable<string>?> ListKnownChainsAcceptingTransactionsAsync() {
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

    public async Task<IEnumerable<string>?> ListKnownChainsAsync()
        => (await _node.GetChainsAsync().ConfigureAwait(false)).Safe().Select(ch => ch.Id);

    public async Task<DocumentsTransactionModel?> TransactionBeginAsync(DocumentsBeginTransactionModel transactionStart) {
        await ValidateChainAsync(transactionStart.Required().Chain).ConfigureAwait(false);
        return await _node.PostAsync<DocumentsTransactionModel>(_transactionPrefix, transactionStart).ConfigureAwait(false);
    }
    public Task<DocumentsTransactionModel?> TransactionAddItemAsync(string transactionId, string path, string name, string? comment, string contentType, Stream source)
        => _node.PostStreamAsync<DocumentsTransactionModel>(UrlFromParts(_transactionPrefix, transactionId.Required(), queryItems: BuildAddItemQueryItems(name, comment)), source.Required(), contentType.Required());
    public Task<string?> TransactionCommitAsync(string transactionId)
        => _node.PostAsync<string>(UrlFromParts(_transactionPrefix, transactionId.Required(), "commit"), body: null);
    public Task<DocumentsTransactionModel?> TransactionStatusAsync(string transactionId)
        => _node.GetAsync<DocumentsTransactionModel>(UrlFromParts(_transactionPrefix, transactionId.Required()));

    public Task<DocumentsMetadataModel?> RetrieveMetadataAsync(string locator)
        => _node.GetAsync<DocumentsMetadataModel>(UrlFromParts(_pathPrefix, locator.Required(), "metadata"));

    Task<(string Name, string ContentType, Stream Content)?> IDocumentRegistry.RetrieveSingleAsync(string locator, int index)
        => _node.GetFileReadStreamAsync(UrlFromParts(_pathPrefix, locator.Required(), index.ToString("0", CultureInfo.InvariantCulture)));

    Task<(string Name, string ContentType, Stream Content)?> IDocumentRegistry.RetrieveZipAsync(string locator, bool omitFromParentControlFile, bool omitToChildrenControlFile)
        => _node.GetFileReadStreamAsync(UrlFromParts(_pathPrefix, locator, "zip", BuildZipQueryItems(omitFromParentControlFile, omitToChildrenControlFile)), accept: "application/zip");

    public static string UrlFromParts(string urlPrefix, string id, string? selector = null, Dictionary<string, string>? queryItems = null) {
        var urlBuilder = new StringBuilder(urlPrefix).Append('/')
                                                     .Append(HttpUtility.UrlEncode(id.Required()));
        if (selector.IsNonBlank())
            urlBuilder.Append('/')
                      .Append(HttpUtility.UrlEncode(selector));
        if (queryItems.SafeAny()) {
            char c = '?';
            foreach (var item in queryItems) {
                urlBuilder.Append(c)
                          .Append(HttpUtility.UrlEncode(item.Key))
                          .Append('=')
                          .Append(HttpUtility.UrlEncode(item.Value));
                c = '&';
            }
        }
        return urlBuilder.ToString();
    }
    private static Dictionary<string, string> BuildAddItemQueryItems(string name, string? comment)
        => new(StringComparer.Ordinal) {
            { nameof(name), name },
            { nameof(comment), comment!},
        };
    private static Dictionary<string, string> BuildZipQueryItems(bool omitFromParentControlFile, bool omitToChildrenControlFile) {
        var queryItems = new Dictionary<string, string>(StringComparer.Ordinal);
        if (omitFromParentControlFile)
            queryItems.Add(nameof(omitFromParentControlFile), "true");
        if (omitToChildrenControlFile)
            queryItems.Add(nameof(omitToChildrenControlFile), "true");
        return queryItems;
    }

    private async Task ValidateChainAsync(string chain) {
        if ((await ListKnownChainsAsync().ConfigureAwait(false)).None(id => string.Equals(id, chain, StringComparison.Ordinal)))
            throw new ArgumentException($"Chain '{chain}' is unknown", nameof(chain));
        if ((await ListKnownChainsAcceptingTransactionsAsync().ConfigureAwait(false)).None(id => string.Equals(id, chain, StringComparison.Ordinal)))
            throw new ArgumentException($"Chain '{chain}' can't accept document transactions", nameof(chain));
    }
}