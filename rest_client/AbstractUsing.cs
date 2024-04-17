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

using System.IO.Compression;
using InterlockLedger.Rest.Client;
using InterlockLedger.Rest.Client.Abstractions;
using InterlockLedger.Rest.Client.V13_7;

namespace rest_client;
public abstract class AbstractUsing<T>(RestAbstractNode<T> node) where T : IRestChain
{
    public static byte[] Content { get; } = Encoding.UTF8.GetBytes("Nothing to see here");
    protected readonly RestAbstractNode<T> _node = node.Required();

    protected abstract string Version { get; }

    protected Task<RecordModelAsJson> AddRecordAsJsonAsync(T chain, ulong appId, ulong action, object json)
        => chain.RecordsAsJson.AddAsync(new NewRecordModelAsJson() { ApplicationId = appId, PayloadTagId = action, Json = json });

    protected Task<RecordModel> AddRecordAsync(T chain, ulong appId, params byte[] payload)
        => chain.Records.AddRecordAsync(new NewRecordModel() { ApplicationId = appId, PayloadBytes = payload });

    private static readonly KeyPermitModel _keyToPermit = new(id: "Key!U0y4av1fQGnOkC_1RkZLd4gE8vVSGVGJO5o1pzprQHo",
                                                              name: "InterlockLedger Documenter",
                                                              publicKey: "PubKey!KPkBERD5AQiuLtsWMFr3H6HtQVUMky1wFzL0TQF3VC-X24G4gjFqcrHHawNxNgDiw21YS8Fx6o1ornUOHqJPvIpYX1H2T2bqbIsIMNgyO4H234Ahken7SadTlnRPw92_sRpqprBobfuX9f9K6iM-SUJ2WY_6U4bAG4HdsFRV4yqfdDhrCAedBUs8O9qyne6vHFN8CiTEcapfQE7K-StPlW2wVmLdIXov2FdfYdJpFLXbbkgBCdkAZl2Oc86PRVzPkqD5dzl86QNZGZxhq2ngQ1UXASUQVh4tV5XqXQoe7xgeiE-1O82oWZWOvH6xdHjY9sMFyY3Mhjz8_MrI_0_DBEH7Pikmhp0LlyucyUA6dz4G_e13Xmyty2LDeqyYNhYORuZu2ev7zIEPvclpKeztC5gmJdCdcXZf_Omigb6I20HiggFBBrTGIjxJ_5xvpfb8DZCB6jqG5deTqybkjDJYPkA0TeoswKlwncT6mmZ3RdNNxoojUEX0TcBfSioKrnWRqGZ6Yc5wPFIvZ2REU6NP5gJv53FYe2yGAFygvWM1t2wBpWb6bx4h4BFKbfHPcCdmPqJHF0WQdMd7rtryENICHh9ozcVHtpHUtGdwoqV8gmeav836canWcXhKWQILiTiLpGAMa7FuUmPUr3K3q0c2rAy0IYXigjHvujTMz_0aGYqZoHD726gb4RADAQAB#RSA",
                                                              new AppPermissions(4).ToEnumerable(),
                                                              KeyPurpose.Protocol,
                                                              KeyPurpose.Action);

    protected abstract Task DoExtraExercisesAsync(RestAbstractNode<T> node, T chain, bool write);

    protected internal async Task ExerciseAsync(bool write) {
        Console.WriteLine($"Client connected to {_node.BaseUri} using certificate '{_node.CertificateName}' via API version {Version}");
        var documentRegistry = (_node is INodeWithDocumentRegistry node) ? node.DocumentRegistry : null;
        if (write) 
            await CreateOneChainAsync();
        Console.WriteLine(await _node.GetDetailsAsync());
        if (documentRegistry is not null)
            Console.WriteLine($" {documentRegistry.GetDocumentsUploadConfigurationAsync().Result}");
        var apps = await _node.Network.GetAppsAsync();
        Console.WriteLine($"-- Valid apps for network {apps.Network}:");
        foreach (var app in apps.ValidApps.OrderByDescending(a => (a.Id, a.AppVersion)).DistinctBy(a => a.Id).OrderBy(a => a.Id))
            Console.WriteLine(app);
        Console.WriteLine();
        var peers = await _node.GetPeersAsync();
        Console.WriteLine($"-- Known peers:");
        foreach (var peer in peers.OrderBy(a => a.Name)) {
            Console.WriteLine(peer);
            Console.WriteLine("---------------------------------");
        }
        Console.WriteLine();
        Console.WriteLine("-- Chains:");
        foreach (var chain in await _node.GetChainsAsync())
            await ExerciseChainAsync(_node, chain, write);
        Console.WriteLine();
        Console.WriteLine("-- Mirrors:");
        foreach (var chain in await _node.GetMirrorsAsync())
            await ExerciseChainAsync(_node, chain, write: false);
        Console.WriteLine();
        if (write) {
            await CreateMirrorsAsync();
            if (documentRegistry is not null)
                await ExerciseDocumentRegistryAsync(documentRegistry);
        }
    }

    private async Task CreateMirrorsAsync() {
        Console.WriteLine("-- Create Mirror:");
        try {
            foreach (var chain in await _node.AddMirrorsOfAsync(_newMirrors))
                Console.WriteLine(chain);
        } catch (Exception e) {
            Console.WriteLine(e.Message);
        }
        Console.WriteLine();
    }

    private async Task CreateOneChainAsync() {
        Console.WriteLine("-- Create Chain:");
        try {
            var chain = await _node.CreateChainAsync(new ChainCreationModel {
                Name = "Rest Created Test Chain",
                Description = "Just a test",
                EmergencyClosingKeyPassword = "password",
                ManagementKeyPassword = "password",
                ManagementKeyStrength = KeyStrength.ExtraStrong,
                KeysAlgorithm = Algorithms.RSA,
                AdditionalApps = [4, 8, 13]
            });
            Console.WriteLine(chain);
         } catch (Exception e) {
            Console.WriteLine(e.Message);
        }
        Console.WriteLine();
    }

    private static async Task ExerciseDocumentRegistryAsync(IDocumentRegistry documentRegistry) {
        var chains = await documentRegistry.ListKnownChainsAcceptingTransactionsAsync();
        Console.WriteLine($"-- Exercise Document Registry (MultiDocuments)");
        Console.WriteLine($"---- {chains.Count()} chains accepting MD transactions");
        var chainId = chains.FirstOrDefault();
        if (chainId is not null) {
            try {
                Console.WriteLine();
                Console.WriteLine("  Trying to begin a transaction:");
                var trx = await documentRegistry.TransactionBeginAsync(new DocumentsBeginTransactionModel {
                    Chain = chainId,
                    Comment = "C# REST client testing",
                    Compression = "BROTLI",
                    Encryption = "PBKDF2-SHA256-AES256-LOW",
                    Password = _password
                });
                Console.WriteLine(trx);
                Console.WriteLine("  Trying to store a nice document:");
                await documentRegistry.TransactionAddItemAsync(trx.TransactionId, "/", "Simple Test 1 Razão.txt", "First file", "text/plain", new MemoryStream(Content, writable: false));
                Console.WriteLine(await documentRegistry.TransactionStatusAsync(trx.TransactionId));
                await documentRegistry.TransactionAddItemAsync(trx.TransactionId, "/", "Simple Test 2 Emoção.txt", "Second file", "text/plain", new MemoryStream(Content, writable: false));
                Console.WriteLine(await documentRegistry.TransactionStatusAsync(trx.TransactionId));
                var locator = await documentRegistry.TransactionCommitAsync(trx.TransactionId);
                Console.WriteLine($"    Documents locator: '{locator}'");
                Console.WriteLine($"    {documentRegistry.RetrieveMetadataAsync(locator)}");
                var secondFile = await documentRegistry.RetrieveSingleAsync(locator, 1);
                if (secondFile is null)
                    Console.WriteLine("Could not retrieve second file");
                else {
                    using var streamReader = new StreamReader(secondFile.Value.Content);
                    Console.WriteLine($"    Retrieved second file {secondFile.Value.Name} : '{streamReader.ReadToEnd()}'");
                    var blobFile = await documentRegistry.RetrieveZipAsync(locator);
                    if (blobFile is null)
                        Console.WriteLine("Could not retrieve zip file");
                    else {
                        using var stream = blobFile.Value.Content;
                        var zip = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: false, Encoding.UTF8);
                        Console.WriteLine($"    Blob file '{blobFile.Value.Name}' contains {zip.Entries.Count} entries.");
                        foreach (var entry in zip.Entries)
                            Console.WriteLine($"    - {entry.FullName}");
                    }
                }
            } catch (Exception e) {
                Console.WriteLine(e.Message);
            }
        }
    }


    protected async Task ExerciseChainAsync(RestAbstractNode<T> node, T chain, bool write) {
        Console.WriteLine(chain);

        var summary = await chain.GetSummaryAsync();
        Console.WriteLine($"  Summary.ActiveApps: {string.Join(", ", summary.ActiveApps)}");
        Console.WriteLine($"  Summary.Description: {summary.Description}");
        Console.WriteLine($"  Summary.IsClosedForNewTransactions: {summary.IsClosedForNewTransactions}");
        Console.WriteLine($"  Summary.LastRecord: {summary.LastRecord}");
        Console.WriteLine($"  Summary.LastUpdate: {summary.LastUpdate}");
        Console.WriteLine($"  Summary.SizeInBytes: {summary.SizeInBytes}");
        Console.WriteLine($"  Summary.LicensingStatus: {summary.LicensingStatus}");
        Console.WriteLine($"  Summary.Licensed: {summary.Licensed}");
        Console.WriteLine($"  Summary.LicenseIsExpired: {summary.LicenseIsExpired}");
        Console.WriteLine($"  Summary.LicensedApps: {summary.LicensedApps.JoinedBy(",")}");
        Console.WriteLine();
        Console.WriteLine($"  Active apps: {string.Join(",", await chain.GetActiveAppsAsync())}");
        Console.WriteLine();
        Console.WriteLine("  Keys:");
        foreach (var key in await chain.GetPermittedKeysAsync())
            Console.WriteLine($"    {key}");
        Console.WriteLine();
        Console.WriteLine("  Interlocks stored here:");
        foreach (var interlock in (await chain.Interlockings.GetInterlocksAsync()).Safe().Items)
            Console.WriteLine($"    {interlock}");
        Console.WriteLine();
        Console.WriteLine("  Interlocks of this chain:");
        foreach (var interlock in (await node.InterlocksOfAsync(chain.Id)).Safe().Items)
            Console.WriteLine($"    {interlock}");
        Console.WriteLine();
        Console.WriteLine("  Records:");
        foreach (var record in (await chain.Records.RecordsFromToAsync(0, 1)).Safe().Items)
            Console.WriteLine($"    {record}");
        Console.WriteLine("  RecordsAsJson:");
        foreach (var record in (await chain.RecordsAsJson.FromToAsync(0, 2)).Safe().Items)
            Console.WriteLine($"    {record}");
        if (write) {
            await TryToAddNiceRecordAsync(chain);
            await TryToAddNiceJsonRecordAsync(chain);
            await TryToAddBadRecordAsync(chain);
            await TryToPermitApp4Async(chain);
            await TryToForceInterlockAsync(chain);
            await TryToPermitKeyAsync(chain);
        }
        Console.WriteLine();
        await DoExtraExercisesAsync(_node, chain, write);
    }


    protected async Task TryToAddBadRecordAsync(T chain) {
        try {
            Console.WriteLine();
            Console.WriteLine("  Trying to add a bad record:");
            var record = await AddRecordAsync(chain, 1, 30);
            Console.WriteLine($"    {record}");
        } catch (Exception e) {
            Console.WriteLine(e.Message);
        }
    }

    protected async Task TryToAddNiceJsonRecordAsync(T chain) {
        try {
            Console.WriteLine();
            Console.WriteLine("  Trying to add a nice JSON record:");
            var record = await AddRecordAsJsonAsync(chain, 1, 300, new { TagId = 300, Version = 1, Apps = new ulong[] { 1, 2, 3 } });
            Console.WriteLine($"    {record}");
        } catch (Exception e) {
            Console.WriteLine(e.Message);
        }
    }

    protected async Task TryToAddNiceRecordAsync(T chain) {
        try {
            Console.WriteLine();
            Console.WriteLine("  Trying to add a nice record:");
            var record = await AddRecordAsync(chain, 1, 248, 52, 10, 5, 0, 0, 20, 5, 4, 0, 1, 2, 3);
            Console.WriteLine($"    {record}");
        } catch (Exception e) {
            Console.WriteLine(e.Message);
        }
    }

    protected async Task TryToForceInterlockAsync(T chain) {
        try {
            Console.WriteLine();
            Console.WriteLine("  Trying to force an interlock:");
            var interlock = await chain.Interlockings.ForceInterlockAsync(new ForceInterlockModel("72_1DyspOtgOpg5XG2ihe7M0xCb2DhrZIQWv3-Bivy4") { HashAlgorithm = HashAlgorithms.Copy, MinSerial = 1 });
            Console.WriteLine($"    {interlock}");
        } catch (Exception e) {
            Console.WriteLine(e.Message);
        }
    }

    protected async Task TryToPermitApp4Async(T chain) {
        try {
            var apps = await chain.PermitAppsAsync(4);
            Console.WriteLine($"  Permit app 4: [{string.Join(", ", apps)}]");
            Console.WriteLine();
        } catch (Exception e) {
            Console.WriteLine(e.Message);
        }
    }

    protected async Task TryToPermitKeyAsync(T chain) {
        try {
            Console.WriteLine();
            Console.WriteLine($"  Trying to permit key: {_keyToPermit.Name} ({_keyToPermit.Id})");
            foreach (var key in await chain.PermitKeysAsync(_keyToPermit))
                Console.WriteLine($"    {key}");
        } catch (Exception e) {
            Console.WriteLine(e.Message);
        }
    }

    protected static async Task ReadSomeJsonDocumentsRecordsAsync<TJson>(X509Certificate2 certificate, TJson jsonStore, IRecordsStore recordsStore, string chainId, ulong payloadTagId, string payloadTypeName, Func<X509Certificate2, TJson, string, ulong, Task> retrieveAndDumpAsync) where TJson : IJsonStore {
        ulong[] filteredRecords = await FindJsonRecords(recordsStore, payloadTagId);
        if (filteredRecords.SafeAny()) {
            Console.WriteLine($"{Environment.NewLine}    ==== {payloadTypeName} from {chainId}: {filteredRecords.Length} records");
            foreach (var serial in filteredRecords) {
                await retrieveAndDumpAsync(certificate, jsonStore, chainId, serial);
            }
        } else {
            Console.WriteLine($"    No {payloadTypeName} found in {chainId}");
        }

        static async Task<ulong[]> FindJsonRecords(IRecordsStore recordsStore, ulong payloadTagId) {
            var records = (await recordsStore.RecordsFromAsync(0, pageSize: 0, lastToFirst: true, ommitPayload: true))?.Items;
            return records.Safe()
                          .Where(r => r.ApplicationId == 8ul && r.PayloadTagId == payloadTagId)
                          .Take(3)
                          .Select(r => r.Serial)
                          .ToArray();
        }

    }

    protected static async Task RetrieveAndDumpJsonDocumentsAsync<TJson>(X509Certificate2 certificate, TJson jsonStore, string chainId, ulong serial) where TJson : IJsonStore {
        var json = await jsonStore.RetrieveAsync(serial);
        Console.WriteLine($"{Environment.NewLine}    Json at {chainId}@{serial}:");
        if (json is null)
            Console.WriteLine("    -- Could not retrieve it!");
        else if (json.JsonText.IsValidJson())
            Console.WriteLine($"    -- {json.JsonText.Ellipsis(300)}");
        else if (json.EncryptedJson is null)
            Console.WriteLine("    -- Invalid format!");
        else {
            Console.WriteLine($"    -- {json.EncryptedJson}");
            Console.WriteLine($"    -- {json.EncryptedJson.DecodedWith(certificate)}");
        }
    }

    private static readonly byte[] _password = Encoding.UTF8.GetBytes("LongEnoughPassword");
    private static readonly string[] _newMirrors = ["72_1DyspOtgOpg5XG2ihe7M0xCb2DhrZIQWv3-Bivy4"];
}