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
using InterlockLedger.Rest.Client.V7_6;

namespace rest_client;

public class UsingV7_6(RestAbstractNode<RestChainV7_6> node) : AbstractUsing<RestChainV7_6>(node)
{
    protected override string Version => "7.6";

    protected override async Task DoExtraExercisesAsync(RestAbstractNode<RestChainV7_6> node, bool write) {
        await ExerciseJsonDocumentAsync(node, write);
        await ExerciseOpaqueRecordsAsync(node, write);
    }

    private async Task ExerciseOpaqueRecordsAsync(RestAbstractNode<RestChainV7_6> node, bool write) {
        var chains = await node.GetChainsAsync();
        foreach (var chain in chains) {
            var opaqueStore = chain.OpaqueStore;
            var query = await opaqueStore.QueryRecordsFromAsync(appId: 13);
            Console.WriteLine($"LastChangedRecordSerial {query.LastChangedRecordSerial} for {chain.Id}");
            try {
                ulong serialToRetrieve = 0;
                if (write) {
                    var result = await opaqueStore.AddRecordAsync(appId: 13, payloadTypeId: 100, query.LastChangedRecordSerial, [1, 2, 3, 4]);
                    serialToRetrieve = result.Serial;
                } else {
                    serialToRetrieve = query.First()?.Serial ?? 0;
                }
                var response = await opaqueStore.RetrieveSinglePayloadAsync(serialToRetrieve);
                Console.WriteLine($"Retrieved AppId: {response.AppId}");
                Console.WriteLine($"Retrieved PayloadTypeId: {response.PayloadTypeId}");
                Console.WriteLine($"Retrieved Bytes: {response.Content.BigEndianReadUInt():x}");
            } catch (Exception ex) {
                if (ex is AggregateException ae) {
                    Console.WriteLine(ae);
                } else {
                    Console.WriteLine(ex);
                }
            }
        }
    }
}