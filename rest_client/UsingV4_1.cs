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
using System.IO;
using System.IO.Compression;
using System.Text;
using InterlockLedger.Rest.Client.Abstractions;
using InterlockLedger.Rest.Client.V4_2;

namespace rest_client
{
    public class UsingV4_1 : AbstractUsing<RestChain>
    {
        public static byte[] Content { get; } = Encoding.UTF8.GetBytes("Nothing to see here");

        public static void DoIt(string[] args) {
            try {
                var client =
                    args.Length > 3
                    ? new RestNode(args[0], args[1], ushort.Parse(args[2]), args[3])
                    : args.Length > 2
                        ? new RestNode(args[0], args[1], ushort.Parse(args[2]))
                        : new RestNode(args[0], args[1]);
                new UsingV4_1(client).Exercise();
            } catch (Exception e) {
                Console.WriteLine(e);
            }
        }

        protected UsingV4_1(RestAbstractNode<RestChain> node) : base(node) {
        }

        protected override string Version => "4.2";

        protected override void DisplayOtherNodeInfo(RestAbstractNode<RestChain> node) {
            if (_node is RestNode nodeV4_2)
                Console.WriteLine($" {nodeV4_2.DocumentsUploadConfiguration}");
        }

        protected override void ExerciseDocApp(RestChain chain) { }

        protected override void TryToStoreNiceDocuments(RestChain chain) {
            if (chain is IDocumentsApp chainDocsApp)
                try {
                    Console.WriteLine();
                    Console.WriteLine("  Trying to begin a transaction:");
                    var trx = chainDocsApp.TransactionBegin(new DocumentsBeginTransactionModel {
                        Chain = chain.Id,
                        Comment = "C# REST client testing",
                        Compression = "BROTLI",
                        Encryption = "PBKDF2-SHA256-AES256-LOW",
                        Password = _password
                    });
                    Console.WriteLine(trx);
                    Console.WriteLine("  Trying to store a nice document:");
                    chainDocsApp.TransactionAddItem(trx.TransactionId, "Simple Test 1 Razão.txt", "First file", "text/plain", new MemoryStream(Content, writable: false));
                    Console.WriteLine(chainDocsApp.TransactionStatus(trx.TransactionId));
                    chainDocsApp.TransactionAddItem(trx.TransactionId, "Simple Test 2 Emoção.txt", "Second file", "text/plain", new MemoryStream(Content, writable: false));
                    Console.WriteLine(chainDocsApp.TransactionStatus(trx.TransactionId));
                    var locator = chainDocsApp.TransactionCommit(trx.TransactionId);
                    Console.WriteLine($"    Documents locator: '{locator}'");
                    Console.WriteLine($"    {chainDocsApp.RetrieveMetadata(locator)}");
                    var secondFile = chainDocsApp.RetrieveSingle(locator, 1, _folderToStore);
                    try {
                        using var streamReader = secondFile.OpenText();
                        Console.WriteLine($"    Retrieved second file {secondFile.FullName} : '{streamReader.ReadToEnd()}'");
                        var blobFile = chainDocsApp.RetrieveBlob(locator, _folderToStore);
                        try {
                            using var stream = blobFile.OpenRead();
                            var zip = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: false, Encoding.UTF8);
                            Console.WriteLine($"    Blob file '{blobFile.FullName}' contains {zip.Entries.Count} entries.");
                            foreach (var entry in zip.Entries)
                                Console.WriteLine($"    - {entry.FullName}");
                        } finally { if (blobFile.Exists) blobFile.Delete(); }
                    } finally { if (secondFile.Exists) secondFile.Delete(); }
                } catch (Exception e) {
                    Console.WriteLine(e);
                }
        }

        private static readonly DirectoryInfo _folderToStore = new DirectoryInfo(Path.GetTempPath());
        private static readonly byte[] _password = Encoding.UTF8.GetBytes("LongEnoughPassword");
    }
}