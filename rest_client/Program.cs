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
                    var client = args.Length > 2 ? new RestClient(args[0], args[1], ushort.Parse(args[2])) : new RestClient(args[0], args[1]);
                    Exercise(client);
                } catch (Exception e) {
                    Console.WriteLine(e);
                }
            }
            do { Console.WriteLine("Press <ESC> to exit!"); } while (Console.ReadKey(intercept: true).Key != ConsoleKey.Escape);
        }

        private static void Exercise(RestClient client) {
            Console.WriteLine($"Client connected to {client.BaseUri} using certificate {client.CertificateName}");
            Console.WriteLine();
            Console.WriteLine(client.NodeDetails);
            AppsModel apps = client.Apps;
            Console.WriteLine($"Valid apps for network {apps.Network}:");
            foreach (var app in apps.ValidApps.OrderBy(a => a))
                Console.WriteLine(app);
            Console.WriteLine();
            foreach (var chain in client.Chains) {
                Console.WriteLine(chain);
                Console.WriteLine($"  Active apps: {string.Join(", ", client.ActiveAppsOn(chain.Id))}");
                Console.WriteLine();
                Console.WriteLine("  Keys:");
                foreach (var key in client.PermittedKeysOn(chain.Id))
                    Console.WriteLine($"    {key}");
                Console.WriteLine();
                Console.WriteLine("  Documents:");
                bool first = true;
                foreach (var doc in client.DocumentsOn(chain.Id)) {
                    Console.WriteLine($"    {doc}");
                    if (first && doc.IsPlainText) {
                        Dump(client.GetPlainDocument(chain.Id, doc.FileId));
                        Dump(client.GetRawDocument(chain.Id, doc.FileId).ToString());
                        first = false;
                    }
                }
                Console.WriteLine();
                Console.WriteLine("  Interlocks:");
                foreach (var interlock in client.InterlocksOn(chain.Id))
                    Console.WriteLine($"    {interlock}");
                Console.WriteLine();
            }
            Console.WriteLine();
        }

        private static void Dump(string document) => Console.WriteLine($"----{Environment.NewLine}{document}{Environment.NewLine}----");
    }
}