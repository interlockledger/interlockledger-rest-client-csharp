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

/// <summary>
/// Model for metadata associated to a Multi-Document Storage Locator
/// </summary>
public class DocumentsMetadataModel
{
    /// <summary>
    /// Replicates RecordReference record info to simplify REST API usage
    /// </summary>
    public string? RecordReference { get; set; }

    /// <summary>
    /// Replicates CreationTime record info to simplify REST API usage
    /// </summary>
    public DateTimeOffset? CreationTime { get; set; }

    /// <summary>
    /// Any additional information about this set of documents
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
    /// The encryption descriptor in the &amp;lt;pbe&amp;gt;-&amp;lt;hash&amp;gt;-&amp;lt;cipher&amp;gt;-&amp;lt;level&amp;gt; format
    /// </summary>
    public string? Encryption { get; set; }

    /// <summary>
    /// Parameters used to do the encryption
    /// </summary>
    public Parameters? EncryptionParameters { get; set; }

    /// <summary>
    /// List of stored documents
    /// </summary>
    public IEnumerable<DirectoryEntry>? PublicDirectory { get; set; }

    public override string ToString() => $$"""
        {{nameof(DocumentsMetadataModel)}} : {
            {{nameof(Comment)}} : {{Comment}}
            {{nameof(Compression)}} : {{Compression}}
            {{nameof(Encryption)}} : {{Encryption}}
            {{nameof(EncryptionParameters)}} : {{EncryptionParameters}}
            {{nameof(PublicDirectory)}} : [{{_joiner}}{{PublicDirectory.JoinedBy(_joiner)}}
            ]
            {{nameof(CreationTime)}} : {{CreationTime:u}}
            {{nameof(RecordReference)}} : {{RecordReference}}
        }
        """;

    /// <summary>
    /// Entry for each stored document in this MultiDocument set
    /// </summary>
    public class DirectoryEntry
    {
        /// <summary>
        /// Any provided additional information about the document file
        /// </summary>
        public string? Comment { get; set; }

        /// <summary>
        /// Mime Type for the document content
        /// </summary>
        public required string MimeType { get; set; }

        /// <summary>
        /// Document (file) name
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Document (file) path (without the name)
        /// </summary>
        public string? Path { get; set; }

        /// <summary>
        /// Hash SHA256 of Document (file) content
        /// </summary>
        public byte[]? HashSHA256 { get; set; }

        /// <summary>
        /// Size in bytes of Document (file)
        /// </summary>
        public ulong? Size { get; set; }

        public override string ToString() => $$"""
            {{nameof(DirectoryEntry)}}: {
                      {{nameof(Comment)}} : {{Comment}}
                      {{nameof(MimeType)}} : {{MimeType}}
                      {{nameof(Name)}} : {{Name}}
                      {{nameof(Path)}} : {{Path}}
                      {{nameof(HashSHA256)}} : {{HashSHA256}}
                      {{nameof(Size)}} : {{Size}}
                  }
            """;
    }

    /// <summary>
    /// The parameters used to make the encryption of the set of documents
    /// </summary>
    public class Parameters
    {
        /// <summary>
        /// Number of iterations to generate the key
        /// </summary>
        public int Iterations { get; set; }

        /// <summary>
        /// Salt value to feed the cypher engine
        /// </summary>
        public byte[]? Salt { get; set; }

        public override string ToString() => $$"""
            {{nameof(Parameters)}} {
                      {{nameof(Iterations)}} : {{Iterations}}
                      {{nameof(Salt)}} : '{{Convert.ToBase64String(Salt ?? [])}}'
                  }
            """;
    }

    private static readonly string _joiner = Environment.NewLine + "      ";
}