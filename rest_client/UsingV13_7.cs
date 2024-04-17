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

using InterlockLedger.Rest.Client.Abstractions;
using InterlockLedger.Rest.Client.V13_7;

namespace rest_client;

public class UsingV13_7(RestAbstractNode<RestChainV13_7> node) : AbstractUsing<RestChainV13_7>(node)
{
    protected override string Version => "13.7";

    protected override async Task DoExtraExercisesAsync(RestAbstractNode<RestChainV13_7> node, RestChainV13_7 chain, bool write) {
        await ExerciseJsonDocumentsV13_7Async(chain, node.Certificate, write);
        await ExerciseOpaqueRecordsAsync(chain, write);
    }

    private static async Task ExerciseJsonDocumentsV13_7Async(RestChainV13_7 chain, X509Certificate2 certificate, bool write) {
        try {
            Console.WriteLine("  JsonDocuments:");
            if (write) {
                // Add something
            }
            await ReadSomeJsonDocumentsRecordsAsync(certificate, chain.JsonStore, chain.Records, chain.Id, 2100, "jsonDocuments", RetrieveAndDumpJsonDocumentsAsync);
            var query = await chain.JsonStore.RetrieveAllowedReadersAsync(chain.Id);
            if (query.TotalNumberOfPages > 0) {
                Console.WriteLine($"    RetrieveAllowedReadersAsync retrieved first page of {query.TotalNumberOfPages} pages with {query.Items.Count()} items");
                Console.WriteLine(query.First()?.AsJson());
            } else {
                Console.WriteLine($"    RetrieveAllowedReadersAsync retrieved no data");
            }
        } catch (Exception e) {
            Console.WriteLine(e.Message);
        }
        Console.WriteLine();
    }

    private static async Task ExerciseOpaqueRecordsAsync(RestChainV13_7 chain, bool write) {
        Console.WriteLine("  OpaqueRecords:");
        var opaqueStore = chain.OpaqueStore;
        var query = await opaqueStore.QueryRecordsFromAsync(appId: 13);
        Console.WriteLine($"    LastChangedRecordSerial {query.LastChangedRecordSerial} for {chain.Id}");
        try {
            ulong serialToRetrieve = 0;
            if (write) {
                Console.WriteLine("    Trying to add an opaque payload #13,100");
                var result = await opaqueStore.AddRecordAsync(appId: 13, payloadTypeId: 100, query.LastChangedRecordSerial, [1, 2, 3, 4]);
                serialToRetrieve = result.Serial;
            } else {
                serialToRetrieve = query.First()?.Serial ?? 0;
            }
            var response = await opaqueStore.RetrieveSinglePayloadAsync(serialToRetrieve);
            if (response.HasValue) {
                Console.WriteLine($"    Retrieved AppId: {response.Value.AppId}");
                Console.WriteLine($"    Retrieved PayloadTypeId: {response.Value.PayloadTypeId}");
                Console.WriteLine($"    Retrieved CreatedAt: {response.Value.CreatedAt}");
                Console.WriteLine($"    Retrieved Bytes: {response.Value.Content.BigEndianReadUInt():x}");
            } else {
                Console.WriteLine("    Could not retrieve opaque payload");
            }
        } catch (Exception ex) {
            if (ex is AggregateException ae) {
                Console.WriteLine(ae.Message);
            } else {
                Console.WriteLine(ex.Message);
            }
        }
        Console.WriteLine();
    }

}