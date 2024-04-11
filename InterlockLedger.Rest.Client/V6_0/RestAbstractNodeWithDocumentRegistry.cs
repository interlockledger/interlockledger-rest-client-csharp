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

public abstract class RestAbstractNodeWithDocumentRegistry<T> : RestAbstractNode<T>, INodeWithDocumentRegistry where T : IRestChain
{
    public RestAbstractNodeWithDocumentRegistry(X509Certificate2 x509Certificate, NetworkPredefinedPorts networkId, string address)
        : base(x509Certificate, networkId, address) { }

    public RestAbstractNodeWithDocumentRegistry(X509Certificate2 x509Certificate, ushort port, string address)
        : base(x509Certificate, port, address) { }

    public RestAbstractNodeWithDocumentRegistry(string certFile, string certPassword, NetworkPredefinedPorts networkId, string address)
        : base(certFile, certPassword, networkId, address) { }

    public RestAbstractNodeWithDocumentRegistry(string certFile, string certPassword, ushort port, string address)
        : base(certFile, certPassword, port, address) { }
    public IDocumentRegistry DocumentRegistry => _documentRegistry ??= new DocumentRegistryImplementation<T>(this);

    private IDocumentRegistry? _documentRegistry;

}
