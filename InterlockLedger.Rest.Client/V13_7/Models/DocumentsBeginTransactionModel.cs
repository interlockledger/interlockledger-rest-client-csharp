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



namespace InterlockLedger.Rest.Client.V13_7;

/// <summary>
/// To specify parameters for starting a transaction to store many documents in a single InterlockLedger record
/// </summary>
public sealed class DocumentsBeginTransactionModel
{
    /// <summary>
    /// Id of the chain where the set of documents should be stored.
    /// </summary>
    public required string Chain { get; set; }

    /// <summary>
    /// Any additional information about the set of documents to be stored
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// Compression algorithm can be:
    ///     <list type="table">
    ///        <item><br/><code>NONE</code><description><para>No compression. Simply store the bytes</para></description></item>
    ///        <item><br/><code>GZIP</code><description><para>Compression of the data using the gzip standard</para></description></item>
    ///        <item><br/><code>BROTLI</code><description><para>Compression of the data using the brotli standard</para></description></item>
    ///        <item><br/><code>ZSTD</code><description><para>Compression of the data using the ZStandard from Facebook (In the future)</para></description></item>
    ///     </list>
    /// </summary>
    public string? Compression { get; set; }

    /// <summary>
    /// The encryption descriptor in the [pbe]-[hash]-[cipher]-[level] format. Ex: "PBKDF2-SHA256-AES256-MID"
    /// </summary>
    public string? Encryption { get; set; }

    /// <summary>
    /// If the publically viewable PublicDirectory field should be created
    /// </summary>
    public bool? GeneratePublicDirectory { get; set; } = true;

    /// <summary>
    /// Override for the number of PBE iterations to generate the key
    /// </summary>
    public int? Iterations { get; set; }

    /// <summary>
    /// Password as bytes if Encryption is not null
    /// </summary>
    public byte[]? Password { get; set; }

    /// <summary>
    /// Locator for the previous version of this set
    /// </summary>
    public string? Previous { get; set; }

    /// <summary>
    /// Indexes of the documents from the previous version of this document set NOT to be copied into this new set (supported since API Version 13.0.0).
    /// If absent/empty all previous documents will be copied
    /// </summary>
    public int[]? PreviousDocumentsNotToCopy { get; set; }

    /// <summary>
    /// For 'Multi-Document Storage Application Chains' embeds '.to-children' control file if true.
    /// </summary>
    public bool? AllowChildren { get; set; }

    /// <summary>
    /// If AllowChildren is true the textual comment to be contained in the '.to-children' control file
    /// </summary>
    public string? ToChildrenComment { get; set; }
}