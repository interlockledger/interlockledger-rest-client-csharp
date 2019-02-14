/******************************************************************************************************************************

Copyright (c) 2018-2019 InterlockLedger Network
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

using InterlockLedger;
using System;
using System.Linq;

namespace rest_client
{
    public static class Program
    {
        public static void Main(string[] args) {
            if (args.Length < 2) {
                Console.WriteLine("You must provide at least 2 parameters!");
                Console.WriteLine();
                Console.WriteLine("Usage: rest_client path-to-certificate-pfx-file certificate-password [api-port]");
            } else {
                try {
                    var client = args.Length > 2 ? new Rest(args[0], args[1], ushort.Parse(args[2])) : new Rest(args[0], args[1]);
                    Exercise(client);
                } catch (Exception e) {
                    Console.WriteLine(e);
                }
            }
            do { Console.WriteLine("Press <ESC> to exit!"); } while (Console.ReadKey(intercept: true).Key != ConsoleKey.Escape);
        }

        private static void Dump(string document) => Console.WriteLine($"----{Environment.NewLine}{document}{Environment.NewLine}----");

        private static void Exercise(Rest client) {
            Console.WriteLine($"Client connected to {client.BaseUri} using certificate {client.CertificateName}");
            Console.WriteLine();
            Console.WriteLine(client.Node_Details);
            AppsModel apps = client.Network_Apps;
            Console.WriteLine($"-- Valid apps for network {apps.Network}:");
            foreach (var app in apps.ValidApps.OrderBy(a => a))
                Console.WriteLine(app);
            Console.WriteLine();
            var peers = client.Node_Peers;
            Console.WriteLine($"-- Known peers:");
            foreach (var peer in peers.OrderBy(a => a.Name))
                Console.WriteLine(peer);
            Console.WriteLine();
            Console.WriteLine("-- Chains:");
            foreach (var chain in client.Node_Chains)
                ExerciseChain(client, chain, transact: true);
            Console.WriteLine();
            Console.WriteLine("-- Mirrors:");
            foreach (var chain in client.Node_Mirrors)
                ExerciseChain(client, chain);
            Console.WriteLine();
        }

        private static void ExerciseChain(Rest client, ChainIdModel chain, bool transact = false) {
            Console.WriteLine(chain);
            Console.WriteLine($"  Active apps: {string.Join(", ", client.Chain_ActiveApps(chain.Id))}");
            Console.WriteLine();
            Console.WriteLine("  Keys:");
            foreach (var key in client.Chain_PermittedKeys(chain.Id))
                Console.WriteLine($"    {key}");
            Console.WriteLine();
            Console.WriteLine("  Documents:");
            bool first = true;
            foreach (var doc in client.Chain_Documents(chain.Id)) {
                Console.WriteLine($"    {doc}");
                if (first && doc.IsPlainText) {
                    Dump(client.Chain_DocumentAsPlain(chain.Id, doc.FileId));
                    Dump(client.Chain_DocumentAsRaw(chain.Id, doc.FileId).ToString());
                    first = false;
                }
            }
            Console.WriteLine();
            Console.WriteLine("  Interlocks stored here:");
            foreach (var interlock in client.Chain_Interlocks(chain.Id))
                Console.WriteLine($"    {interlock}");
            Console.WriteLine();
            Console.WriteLine("  Interlocks of this chain:");
            foreach (var interlock in client.Node_InterlocksOf(chain.Id))
                Console.WriteLine($"    {interlock}");
            Console.WriteLine();
            Console.WriteLine("  Records:");
            foreach (var record in client.Chain_Records(chain.Id, 0, 1))
                Console.WriteLine($"    {record}");
            if (transact) {
                TryToForceInterlock(client, chain);
                TryToPermitKey(client, chain);
            }
            Console.WriteLine();
        }

        private static void TryToForceInterlock(Rest client, ChainIdModel chain) {
            try {
                Console.WriteLine();
                Console.WriteLine("  Trying to force an interlock:");
                var interlock = client.Chain_ForceInterlock(chain.Id, new ForceInterlockModel() { HashAlgorithm = HashAlgorithms.Copy, MinSerial = 1, TargetChain = "72_1DyspOtgOpg5XG2ihe7M0xCb2DhrZIQWv3-Bivy4" });
                Console.WriteLine($"    {interlock}");
            } catch (Exception e) {
                Console.WriteLine(e);
            }
        }

        private static void TryToPermitKey(Rest client, ChainIdModel chain) {
            try {
                Console.WriteLine();
                Console.WriteLine("  Trying to permit some keys:");
                foreach (var key in client.Chain_PermittedKeys(chain.Id,
                        new KeyPermitModel(4, new ulong[] { 1000, 1001 },
                        "Key!U0y4av1fQGnOkC_1RkZLd4gE8vVSGVGJO5o1pzprQHo", "InterlockLedger Documenter",
                        "PubKey!KPkBERD5AQiuLtsWMFr3H6HtQVUMky1wFzL0TQF3VC-X24G4gjFqcrHHawNxNgDiw21YS8Fx6o1ornUOHqJPvIpYX1H2T2bqbIsIMNgyO4H234Ahken7SadTlnRPw92_sRpqprBobfuX9f9K6iM-SUJ2WY_6U4bAG4HdsFRV4yqfdDhrCAedBUs8O9qyne6vHFN8CiTEcapfQE7K-StPlW2wVmLdIXov2FdfYdJpFLXbbkgBCdkAZl2Oc86PRVzPkqD5dzl86QNZGZxhq2ngQ1UXASUQVh4tV5XqXQoe7xgeiE-1O82oWZWOvH6xdHjY9sMFyY3Mhjz8_MrI_0_DBEH7Pikmhp0LlyucyUA6dz4G_e13Xmyty2LDeqyYNhYORuZu2ev7zIEPvclpKeztC5gmJdCdcXZf_Omigb6I20HiggFBBrTGIjxJ_5xvpfb8DZCB6jqG5deTqybkjDJYPkA0TeoswKlwncT6mmZ3RdNNxoojUEX0TcBfSioKrnWRqGZ6Yc5wPFIvZ2REU6NP5gJv53FYe2yGAFygvWM1t2wBpWb6bx4h4BFKbfHPcCdmPqJHF0WQdMd7rtryENICHh9ozcVHtpHUtGdwoqV8gmeav836canWcXhKWQILiTiLpGAMa7FuUmPUr3K3q0c2rAy0IYXigjHvujTMz_0aGYqZoHD726gb4RADAQAB#RSA",
                        KeyPurpose.Protocol, KeyPurpose.Action)))
                    Console.WriteLine($"    {key}");
            } catch (Exception e) {
                Console.WriteLine(e);
            }
        }
    }
}