// ******************************************************************************************************************************
//
// Copyright (c) 2018-2021 InterlockLedger Network
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
using InterlockLedger.Rest.Client.V6_0;

namespace rest_client;

public abstract class AbstractUsing<T> where T : IRestChain
{
    public static byte[] Content { get; } = Encoding.UTF8.GetBytes("Nothing to see here");
    protected readonly RestAbstractNode<T> _node;

    protected AbstractUsing(RestAbstractNode<T> node) => _node = node.Required(nameof(node));

    protected abstract string Version { get; }

    protected Task<RecordModelAsJson> AddRecordAsJsonAsync(T chain, ulong appId, ulong action, object json)
        => chain.RecordsAsJson.AddAsync(new NewRecordModelAsJson() { ApplicationId = appId, PayloadTagId = action, Json = json });

    protected Task<RecordModel> AddRecordAsync(T chain, ulong appId, params byte[] payload)
        => chain.Records.AddRecordAsync(new NewRecordModel() { ApplicationId = appId, PayloadBytes = payload });

    protected KeyPermitModel BuildKey()
        => new(
            id: "Key!U0y4av1fQGnOkC_1RkZLd4gE8vVSGVGJO5o1pzprQHo",
            name: "InterlockLedger Documenter",
            publicKey: "PubKey!KPkBERD5AQiuLtsWMFr3H6HtQVUMky1wFzL0TQF3VC-X24G4gjFqcrHHawNxNgDiw21YS8Fx6o1ornUOHqJPvIpYX1H2T2bqbIsIMNgyO4H234Ahken7SadTlnRPw92_sRpqprBobfuX9f9K6iM-SUJ2WY_6U4bAG4HdsFRV4yqfdDhrCAedBUs8O9qyne6vHFN8CiTEcapfQE7K-StPlW2wVmLdIXov2FdfYdJpFLXbbkgBCdkAZl2Oc86PRVzPkqD5dzl86QNZGZxhq2ngQ1UXASUQVh4tV5XqXQoe7xgeiE-1O82oWZWOvH6xdHjY9sMFyY3Mhjz8_MrI_0_DBEH7Pikmhp0LlyucyUA6dz4G_e13Xmyty2LDeqyYNhYORuZu2ev7zIEPvclpKeztC5gmJdCdcXZf_Omigb6I20HiggFBBrTGIjxJ_5xvpfb8DZCB6jqG5deTqybkjDJYPkA0TeoswKlwncT6mmZ3RdNNxoojUEX0TcBfSioKrnWRqGZ6Yc5wPFIvZ2REU6NP5gJv53FYe2yGAFygvWM1t2wBpWb6bx4h4BFKbfHPcCdmPqJHF0WQdMd7rtryENICHh9ozcVHtpHUtGdwoqV8gmeav836canWcXhKWQILiTiLpGAMa7FuUmPUr3K3q0c2rAy0IYXigjHvujTMz_0aGYqZoHD726gb4RADAQAB#RSA",
            new AppPermissions(4).ToEnumerable(),
            KeyPurpose.Protocol,
            KeyPurpose.Action);

    protected void DisplayOtherNodeInfo() {
        if (_node is IDocumentRegistry nodeV4_2)
            Console.WriteLine($" {nodeV4_2.GetDocumentsUploadConfigurationAsync().Result}");
    }

    protected abstract Task DoExtraExercisesAsync(RestAbstractNode<T> node, bool write);

    protected void Dump(string document) => Console.WriteLine($"----{Environment.NewLine}{document}{Environment.NewLine}----");

    protected async Task ExerciseAsync(bool write) {
        Console.WriteLine($"Client connected to {_node.BaseUri} using certificate {_node.CertificateName} using API version {Version}");
        if (write) {
            Console.WriteLine("-- Create Chain:");
            try {
                var chain = await _node.CreateChainAsync(new ChainCreationModel {
                    Name = "Rest Created Test Chain",
                    Description = "Just a test",
                    EmergencyClosingKeyPassword = "password",
                    ManagementKeyPassword = "password",
                    ManagementKeyStrength = KeyStrength.ExtraStrong,
                    KeysAlgorithm = Algorithms.RSA,
                    AdditionalApps = new List<ulong> { 4, 8 }
                });
                Console.WriteLine(chain);
            } catch (InvalidOperationException) {
                throw;
            } catch (Exception e) {
                Console.WriteLine(e);
            }
            Console.WriteLine();
        }
        Console.WriteLine(await _node.GetDetailsAsync());
        DisplayOtherNodeInfo();
        var apps = await _node.Network.GetAppsAsync();
        Console.WriteLine($"-- Valid apps for network {apps.Network}:");
        foreach (var app in apps.ValidApps.OrderBy(a => a))
            Console.WriteLine(app);
        Console.WriteLine();
        var peers = await _node.GetPeersAsync();
        Console.WriteLine($"-- Known peers:");
        foreach (var peer in peers.OrderBy(a => a.Name))
            Console.WriteLine(peer);
        Console.WriteLine();
        Console.WriteLine("-- Chains:");
        foreach (var chain in await _node.GetChainsAsync())
            await ExerciseChainAsync(_node, chain, transact: write);
        Console.WriteLine();
        Console.WriteLine("-- Mirrors:");
        foreach (var chain in await _node.GetMirrorsAsync())
            await ExerciseChainAsync(_node, chain);
        Console.WriteLine();
        if (write) {
            Console.WriteLine("-- Create Mirror:");
            try {
                foreach (var chain in await _node.AddMirrorsOfAsync(new string[] { "72_1DyspOtgOpg5XG2ihe7M0xCb2DhrZIQWv3-Bivy4" }))
                    Console.WriteLine(chain);
            } catch (Exception e) {
                Console.WriteLine(e);
            }
            Console.WriteLine();
        }
        await DoExtraExercisesAsync(_node, write);
    }

    protected async Task ExerciseChainAsync(RestAbstractNode<T> node, T chain, bool transact = false) {
        Console.WriteLine(chain);

        var summary = await chain.GetSummaryAsync();
        Console.WriteLine($"  Summary.ActiveApps: {string.Join(", ", summary.ActiveApps)}");
        Console.WriteLine($"  Summary.Description: {summary.Description}");
        Console.WriteLine($"  Summary.IsClosedForNewTransactions: {summary.IsClosedForNewTransactions}");
        Console.WriteLine($"  Summary.LastRecord: {summary.LastRecord}");
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
        if (transact) {
            await TryToAddNiceUnpackedRecordAsync(chain);
            await TryToAddNiceRecordAsync(chain);
            await TryToAddNiceJsonRecordAsync(chain);
            await TryToAddBadlyEncodedUnpackedRecordAsync(chain);
            await TryToAddBadRecordAsync(chain);
            await TryToPermitApp4Async(chain);
            await TryToStoreNiceDocumentsAsync(chain);
            await TryToForceInterlockAsync(chain);
            await TryToPermitKeyAsync(chain);
        }
        Console.WriteLine();
    }

    protected async Task TryToAddBadlyEncodedUnpackedRecordAsync(T chain) {
        try {
            Console.WriteLine();
            Console.WriteLine("  Trying to add a badly encoded unpacked record:");
            var record = await chain.Records.AddRecordAsync(1, 300, new byte[] { 10, 5, 0, 0, 20, 5, 4, 0, 1, 2, 3 });
            Console.WriteLine($"    {record}");
        } catch (Exception e) {
            Console.WriteLine(e);
        }
    }

    protected async Task TryToAddBadRecordAsync(T chain) {
        try {
            Console.WriteLine();
            Console.WriteLine("  Trying to add a bad record:");
            var record = await AddRecordAsync(chain, 1, 0);
            Console.WriteLine($"    {record}");
        } catch (Exception e) {
            Console.WriteLine(e);
        }
    }

    protected async Task TryToAddNiceJsonRecordAsync(T chain) {
        try {
            Console.WriteLine();
            Console.WriteLine("  Trying to add a nice JSON record:");
            var record = await AddRecordAsJsonAsync(chain, 1, 300, new { TagId = 300, Version = 1, Apps = new ulong[] { 1, 2, 3 } });
            Console.WriteLine($"    {record}");
        } catch (Exception e) {
            Console.WriteLine(e);
        }
    }

    protected async Task TryToAddNiceRecordAsync(T chain) {
        try {
            Console.WriteLine();
            Console.WriteLine("  Trying to add a nice record:");
            var record = await AddRecordAsync(chain, 1, 248, 52, 10, 5, 0, 0, 20, 5, 4, 0, 1, 2, 3);
            Console.WriteLine($"    {record}");
        } catch (Exception e) {
            Console.WriteLine(e);
        }
    }

    protected async Task TryToAddNiceUnpackedRecordAsync(T chain) {
        try {
            Console.WriteLine();
            Console.WriteLine("  Trying to add a nice unpacked record:");
            var record = await chain.Records.AddRecordAsync(1, 300, new byte[] { 5, 0, 0, 20, 5, 4, 0, 1, 2, 3 });
            Console.WriteLine($"    {record}");
        } catch (Exception e) {
            Console.WriteLine(e);
        }
    }

    protected async Task TryToForceInterlockAsync(T chain) {
        try {
            Console.WriteLine();
            Console.WriteLine("  Trying to force an interlock:");
            var interlock = await chain.Interlockings.ForceInterlockAsync(new ForceInterlockModel() { HashAlgorithm = HashAlgorithms.Copy, MinSerial = 1, TargetChain = "72_1DyspOtgOpg5XG2ihe7M0xCb2DhrZIQWv3-Bivy4" });
            Console.WriteLine($"    {interlock}");
        } catch (Exception e) {
            Console.WriteLine(e);
        }
    }

    protected async Task TryToPermitApp4Async(T chain) {
        try {
            var apps = await chain.PermitAppsAsync(4);
            Console.WriteLine($"  Permit app 4: {string.Join(", ", apps)}");
            Console.WriteLine();
        } catch (Exception e) {
            Console.WriteLine(e);
        }
    }

    protected async Task TryToPermitKeyAsync(T chain) {
        try {
            Console.WriteLine();
            Console.WriteLine("  Trying to permit some keys:");
            foreach (var key in await chain.PermitKeysAsync(BuildKey()))
                Console.WriteLine($"    {key}");
        } catch (Exception e) {
            Console.WriteLine(e);
        }
    }

    protected async Task TryToStoreNiceDocumentsAsync(T chain) {
        if (_node is IDocumentRegistry docsApp)
            try {
                Console.WriteLine();
                Console.WriteLine("  Trying to begin a transaction:");
                var trx = await docsApp.TransactionBeginAsync(new DocumentsBeginTransactionModel {
                    Chain = chain.Id,
                    Comment = "C# REST client testing",
                    Compression = "BROTLI",
                    Encryption = "PBKDF2-SHA256-AES256-LOW",
                    Password = _password
                });
                Console.WriteLine(trx);
                Console.WriteLine("  Trying to store a nice document:");
                await docsApp.TransactionAddItemAsync(trx.TransactionId, "/", "Simple Test 1 Razão.txt", "First file", "text/plain", new MemoryStream(Content, writable: false));
                Console.WriteLine(await docsApp.TransactionStatusAsync(trx.TransactionId));
                await docsApp.TransactionAddItemAsync(trx.TransactionId, "/", "Simple Test 2 Emoção.txt", "Second file", "text/plain", new MemoryStream(Content, writable: false));
                Console.WriteLine(await docsApp.TransactionStatusAsync(trx.TransactionId));
                var locator = await docsApp.TransactionCommitAsync(trx.TransactionId);
                Console.WriteLine($"    Documents locator: '{locator}'");
                Console.WriteLine($"    {docsApp.RetrieveMetadataAsync(locator)}");
                var secondFile = await docsApp.RetrieveSingleAsync(locator, 1);
                using var streamReader = new StreamReader(secondFile.Content);
                Console.WriteLine($"    Retrieved second file {secondFile.Name} : '{streamReader.ReadToEnd()}'");
                var blobFile = await docsApp.RetrieveZipAsync(locator);
                using var stream = blobFile.Content;
                var zip = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: false, Encoding.UTF8);
                Console.WriteLine($"    Blob file '{blobFile.Name}' contains {zip.Entries.Count} entries.");
                foreach (var entry in zip.Entries)
                    Console.WriteLine($"    - {entry.FullName}");
            } catch (Exception e) {
                Console.WriteLine(e);
            }
    }

    private static readonly byte[] _password = Encoding.UTF8.GetBytes("LongEnoughPassword");
}