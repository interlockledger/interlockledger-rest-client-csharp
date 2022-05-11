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

namespace InterlockLedger.Rest.Client;

public class KeyPermitModel
{
    public KeyPermitModel(string id, string name, string publicKey, ulong app, IEnumerable<ulong> appActions, params KeyPurpose[] purposes)
        : this(id, name, publicKey, new AppPermissions[] { new AppPermissions(app, appActions) }, purposes) {
    }

    public KeyPermitModel(string id, string name, string publicKey, IEnumerable<AppPermissions> permissions, params KeyPurpose[] purposes) {
        Permissions = permissions.Required();
        if (!permissions.Any())
            throw new InvalidDataException("This key doesn't have at least one action to be permitted");
        Id = id.Required();
        Name = name.Required();
        PublicKey = publicKey.Required();
        Purposes = purposes.Required();
        if (!(purposes.Contains(KeyPurpose.Action) && purposes.Contains(KeyPurpose.Protocol)))
            throw new InvalidDataException("This key doesn't have the required purposes to be permitted");
    }

    /// <summary>
    /// Unique key id
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Key name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// List of Apps and Corresponding Actions to be permitted by numbers
    /// </summary>
    public IEnumerable<AppPermissions> Permissions { get; set; }

    /// <summary>
    /// Key public key
    /// </summary>
    public string PublicKey { get; set; }

    /// <summary>
    /// Key valid purposes
    /// </summary>
    public KeyPurpose[] Purposes { get; set; }
}