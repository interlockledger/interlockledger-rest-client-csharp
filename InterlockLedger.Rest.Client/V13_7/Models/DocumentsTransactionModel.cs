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

public class DocumentsTransactionModel
{
    /// <summary>
    /// If no files/documents are still uploading
    /// </summary>
    public bool CanCommitNow { get; set; }

    /// <summary>
    /// Id of chain where the transaction data will be stored
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
    /// Total count of uploaded documents for this transaction
    /// </summary>
    public int CountOfUploadedDocuments { get; set; }

    /// <summary>
    /// Names of documents already uploaded
    /// </summary>
    public IEnumerable<string> DocumentNames { get; set; } = [];

    /// <summary>
    /// The encryption descriptor in the &amp;lt;pbe&amp;gt;-&amp;lt;hash&amp;gt;-&amp;lt;cipher&amp;gt;-&amp;lt;level&amp;gt; format
    /// </summary>
    public string? Encryption { get; set; }

    /// <summary>
    /// If the publically viewable PublicDirectory field should be created
    /// </summary>
    public bool GeneratePublicDirectory { get; set; }

    /// <summary>
    /// Locator for the previous version of this set
    /// </summary>
    public string? Previous { get; set; }

    /// <summary>
    /// The transaction will be aborted if not completed until this timeout
    /// </summary>
    public DateTimeOffset TimeOutLimit { get; set; }

    /// <summary>
    /// Id of the transaction to use when uploading each file and committing the transaction
    /// </summary>
    public string? TransactionId { get; set; }
}