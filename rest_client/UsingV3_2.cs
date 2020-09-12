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
using System.Text;
using InterlockLedger.Rest.Client.Abstractions;
using InterlockLedger.Rest.Client.V3_2;

namespace rest_client
{
    public class UsingV3_2 : AbstractUsing<RestChain>
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
                new UsingV3_2(client).Exercise();
            } catch (Exception e) {
                Console.WriteLine(e);
            }
        }

        protected UsingV3_2(RestAbstractNode<RestChain> node) : base(node) {
        }

        protected override string Version => "3.2";

        protected override void DisplayOtherNodeInfo(RestAbstractNode<RestChain> node) {
            if (_node is RestNode nodeV3_2)
                Console.WriteLine($" {nodeV3_2.MultiDocumentUploadConfiguration}");
        }

        protected override void ExerciseDocApp(RestChain chain) { }

        protected override void TryToStoreNiceDocuments(RestChain chain) {
            if (chain is IMultiDocumentApp chainDocApp)
                try {
                    Console.WriteLine();
                    Console.WriteLine("  Trying to begin a transaction:");
                    var trx = chainDocApp.BeginTransaction(new MultiDocumentBeginTransactionModel {
                        Chain = chain.Id,
                        Comment = "C# client testing",
                        Password = _password
                    });
                    Console.WriteLine(trx);
                    Console.WriteLine("  Trying to store a nice document:");
                    chainDocApp.AddItem(trx.TransactionId, "Simple Test.txt", "First file", "text/plain", new MemoryStream(Content, writable: false));
                    var locator = chainDocApp.CommitTransaction(trx.TransactionId);
                    Console.WriteLine($"    {locator}");
                    Console.WriteLine($"    {chainDocApp.RetrieveMetadata(locator)}");
                } catch (Exception e) {
                    Console.WriteLine(e);
                }
        }

        private static readonly byte[] _password = Encoding.UTF8.GetBytes("LongEnoughPassword");
    }
}