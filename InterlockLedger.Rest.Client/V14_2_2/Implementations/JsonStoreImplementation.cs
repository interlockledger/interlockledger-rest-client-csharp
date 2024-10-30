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

internal sealed class JsonStoreImplementation : IJsonStore {
    public JsonStoreImplementation(RestChainV14_2_2 parent) {
        _chain = parent.Required();
        _node = _chain._node;
        ChainId = _chain.Id;
    }
    public bool IsWritable => _chain.CanWriteForAppAsync(8).Result;
    public string NetworkName => _node.NetworkName;
    public string ChainId { get; }

    public Task<JsonDocumentModel?> Retrieve(ulong serial)
        => _node.GetAsync<JsonDocumentModel>($"/jsonDocuments@{ChainId}/{serial}");

    public async Task<T?> RetrieveDecodedAs<T>(ulong serial, X509Certificate2? certificate = null) where T : class {
        var jsonDocument = await Retrieve(serial).ConfigureAwait(false);
        return jsonDocument?.EncryptedJson?.DecodedAs<T?>(certificate ?? _node.Certificate);
    }

    public Task<PageOfAllowedReadersRecordModel?> RetrieveAllowedReaders(string chain, string? contextId = null, bool lastToFirst = false, int page = 0, int pageSize = 10)
        => _node.GetAsync<PageOfAllowedReadersRecordModel>($"/jsonDocuments@{ChainId}/allow?lastToFirst={lastToFirst}&page={page}&pageSize={pageSize}&contextId={contextId}");
    public Task<JsonDocumentModel?> Add<T>(T jsonDocument)
        => _node.PostAsync<JsonDocumentModel>($"/jsonDocuments@{ChainId}/", jsonDocument);
    public Task<JsonDocumentModel?> Add<T>(T jsonDocument, PublicKey readerKey, string readerKeyId)
        => _node.PostAsync<JsonDocumentModel>($"/jsonDocuments@{ChainId}/withKey",
                                              jsonDocument,
                                              extraHeaders: [$"X-PubKey: {readerKey.ToInterlockLedgerPublicKeyRepresentation()}", $"X-PubKeyId: {readerKeyId}"]);
    public Task<JsonDocumentModel?> Add<T>(T jsonDocument, RecordReference[] allowedReadersReferences) =>
        _node.PostAsync<JsonDocumentModel>($"/jsonDocuments@{ChainId}/withIndirectKeys",
                                           jsonDocument,
                                           extraHeaders: [$"X-PubKeyReferences: {allowedReadersReferences.NonEmpty().JoinedBy(",")}"]);

    public Task<JsonDocumentModel?> Add<T>(T jsonDocument, string[] idOfChainsWithAllowedReaders)
        => _node.PostAsync<JsonDocumentModel>($"/jsonDocuments@{ChainId}/withChainKeys",
                                              jsonDocument,
                                              extraHeaders: [$"X-PubKeyChains: {idOfChainsWithAllowedReaders.NonEmpty().JoinedBy(",")}"]);
    public Task<AllowedReadersRecordModel?> AddAllowedReaders(AllowedReadersModel allowedReaders)
        => _node.PostAsync<AllowedReadersRecordModel>($"/jsonDocuments@{ChainId}/allow", allowedReaders);

    private readonly RestChainV14_2_2 _chain;
    private readonly RestAbstractNode<RestChainV14_2_2> _node;

}