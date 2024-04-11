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

using Cocona;
using InterlockLedger.Rest.Client.V6_0;
using InterlockLedger.Rest.Client.V7_6;
using rest_client;


CoconaLiteApp.Run(([Argument(Description = "Path to file containing client certificate in .pfx format")] string pathToCertificatePfxFile,
                   [Argument(Description = "Password to open file containing client certificate")] string certificatePassword,
                   [Argument(Description = "Port to access the node API (ex.: 32032)")] ushort apiPort,
                   [Argument(Description = "Address to access the node API (ex.: node.il2, localhost)")] string restURL,
                   [Option(Description = "Try to write into the node (Default: false)")] bool writeable = false,
                   [Option(Description = "API version to exercise in the node")] KnownVersions version = KnownVersions.V7_6) => {
                       try {
                           var pathToCertificatePfxFileInfo = new FileInfo(pathToCertificatePfxFile);
                           if (!pathToCertificatePfxFileInfo.Exists) {
                               Console.WriteLine($"File {pathToCertificatePfxFile} not found!");
                               return 2;
                           }
                           switch (version) {
                           case KnownVersions.V6_0:
                               var client6_0 = new RestNodeV6_0(pathToCertificatePfxFileInfo.FullName, certificatePassword, apiPort, restURL);
                               new UsingV6_0(client6_0).ExerciseAsync(writeable).Wait();
                               break;
                           case KnownVersions.V7_6:
                               var client7_6 = new RestNodeV7_6(pathToCertificatePfxFileInfo.FullName, certificatePassword, apiPort, restURL);
                               new UsingV7_6(client7_6).ExerciseAsync(writeable).Wait();
                               break;
                           default:
                               Console.WriteLine($"No known API version chosen '{version}'");
                               break;
                           }
                           return 0;
                       } catch (Exception e) {
                           Console.WriteLine(e);
                           return 1;
                       }
                   }, configureOptions: options => {
                       options.EnableConvertArgumentNameToLowerCase = false;
                       options.EnableConvertOptionNameToLowerCase = false;
                       options.TreatPublicMethodsAsCommands = false;
                   });
