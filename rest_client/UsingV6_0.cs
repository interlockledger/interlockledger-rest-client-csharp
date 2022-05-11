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

using InterlockLedger.Rest.Client;
using InterlockLedger.Rest.Client.Abstractions;
using InterlockLedger.Rest.Client.V6_0;

namespace rest_client;

public class UsingV6_0 : AbstractUsing<RestChain>
{
    public static void DoIt(string[] args) {
        try {
            var client =
                args.Length > 3
                ? new RestNode(args[0], args[1], ushort.Parse(args[2]), args[3])
                : throw new InvalidOperationException("You must provide at least 4 parameters");
            new UsingV6_0(client).ExerciseAsync(args.Length > 4).Wait();
        } catch (Exception e) {
            Console.WriteLine(e);
        }
    }

    protected UsingV6_0(RestAbstractNode<RestChain> node) : base(node) {
    }

    protected override string Version => "6.0";

    protected override async Task DoExtraExercisesAsync(RestAbstractNode<RestChain> node, bool write) {
        try {
            foreach (IRestChainV6_0 chain in await node.GetChainsAsync()) {
                if (write) {
                    // Add something
                }
                var records = await chain.Records.RecordsFromAsync(0, pageSize: 0);
                var filteredRecords = records.Items.Where(r => r.ApplicationId == 8ul && r.PayloadTagId == 2100)
                                                   .Select(r => r.Serial)
                                                   .ToArray();
                Console.WriteLine($"{Environment.NewLine}==== JSON from {chain.Id}: {filteredRecords.Length} records");
                foreach (var serial in filteredRecords) {
                    var json = await chain.JsonStore.RetrieveAsync(serial);
                    Console.WriteLine($"{Environment.NewLine}Json at {chain.Id}@{serial}:");
                    if (json is null)
                        Console.WriteLine("-- Could not retrieve it!");
                    else if (json.JsonText.IsValidJson())
                        Console.WriteLine($"-- {json.JsonText.Ellipsis(300)}");
                    else if (json.EncryptedJson is null)
                        Console.WriteLine("-- Invalid format!");
                    else {
                        Console.WriteLine($"-- {json.EncryptedJson}");
                        Console.WriteLine($"-- {json.EncryptedJson.DecodedWith(node.Certificate)}");
                    }
                }
            }
        } catch (Exception e) {
            Console.WriteLine(e);
        }
    }
}