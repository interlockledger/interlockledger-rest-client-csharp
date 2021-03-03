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
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Web;
using InterlockLedger.Rest.Client.Abstractions;

namespace InterlockLedger.Rest.Client.V6_0
{
    public class RestNode : RestAbstractNode<RestChain>, IDocumentsApp
    {
        public RestNode(X509Certificate2 x509Certificate, NetworkPredefinedPorts networkId, string address)
            : base(x509Certificate, networkId, address) { }

        public RestNode(X509Certificate2 x509Certificate, ushort port, string address)
            : base(x509Certificate, port, address) { }

        public RestNode(string certFile, string certPassword, NetworkPredefinedPorts networkId, string address)
            : base(certFile, certPassword, networkId, address) { }

        public RestNode(string certFile, string certPassword, ushort port, string address)
            : base(certFile, certPassword, port, address) { }

        Task<DocumentsUploadConfiguration> IDocumentsApp.GetDocumentsUploadConfigurationAsync()
            => GetAsync<DocumentsUploadConfiguration>("/documents/configuration");

        public async Task<IEnumerable<string>> ListKnownChainsAcceptingTransactionsAsync() {
            var result = new List<string>();
            foreach (var chain in (await GetChainsAsync().ConfigureAwait(false)).Safe()) {
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
            => (await GetChainsAsync().ConfigureAwait(false)).Safe().Select(ch => ch.Id);

        Task<DocumentsMetadataModel> IDocumentsApp.RetrieveMetadataAsync(string locator)
            => GetAsync<DocumentsMetadataModel>(FromLocator(locator.Required(nameof(locator)), "metadata"));

        Task<(string Name, string ContentType, Stream Content)> IDocumentsApp.RetrieveSingleAsync(string locator, int index)
            => GetFileReadStreamAsync(FromLocator(locator.Required(nameof(locator)), index));

        Task<(string Name, string ContentType, Stream Content)> IDocumentsApp.RetrieveZipAsync(string locator)
            => GetFileReadStreamAsync(FromLocator(locator.Required(nameof(locator)), "zip"), accept: "application/zip");

        Task<DocumentsTransactionModel> IDocumentsApp.TransactionAddItemAsync(string transactionId, string path, string name, string comment, string contentType, Stream source)
            => PostStreamAsync<DocumentsTransactionModel>($"/documents/transaction/{transactionId.Required(nameof(transactionId))}?name={HttpUtility.UrlEncode(name.Required(nameof(name)))}&comment={HttpUtility.UrlEncode(comment)}", source.Required(nameof(source)), contentType.Required(nameof(contentType)));

        async Task<DocumentsTransactionModel> IDocumentsApp.TransactionBeginAsync(DocumentsBeginTransactionModel transactionStart) {
            await ValidateChainAsync(transactionStart.Required(nameof(transactionStart)).Chain);
            return await PostAsync<DocumentsTransactionModel>("/documents/transaction", transactionStart);
        }

        Task<string> IDocumentsApp.TransactionCommitAsync(string transactionId)
            => PostAsync<string>($"/documents/transaction/{transactionId.Required(nameof(transactionId))}/commit", null);

        Task<DocumentsTransactionModel> IDocumentsApp.TransactionStatusAsync(string transactionId)
            => GetAsync<DocumentsTransactionModel>($"/documents/transaction/{transactionId.Required(nameof(transactionId))}");

        protected override RestChain BuildChain(ChainIdModel c) => new RestChain(this, c.Required(nameof(c)));

        private static string FromLocator<T>(string locator, T selector) => $"/documents/{HttpUtility.UrlEncode(locator)}/{selector}";

        private async Task ValidateChainAsync(string chain) {
            if ((await ListKnownChainsAsync()).None(id => id == chain))
                throw new ArgumentException($"Chain '{chain}' is unknown");
            if ((await ListKnownChainsAcceptingTransactionsAsync()).None(id => id == chain))
                throw new ArgumentException($"Chain '{chain}' can't accept document transactions");
        }
    }
}