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
using InterlockLedger.Rest.Client;
using InterlockLedger.Rest.Client.V14_2_2;
using rest_client;

await CoconaLiteApp.RunAsync(static async (
    [Argument(Description = "Connection string in the form \"https://minerva-data.il2.io[:32067],/absolute/path/to/clientcertificate.pfx,clientCertificatePassword[,minerva]\"")] string connectionString,
    [Option(Description = "Try to write into the node (Default: false)")] bool writeable = false) => {
        try {
            var client = new RestNodeV14_2_2(new ConnectionString(connectionString));
            await new UsingV14_2_2(client).ExerciseAsync(writeable).ConfigureAwait(false);
            return 0;
        } catch (Exception e) {
            Console.WriteLine(e);
            return 1;
        }
    }, configureOptions: options => {
        options.EnableConvertArgumentNameToLowerCase = false;
        options.EnableConvertOptionNameToLowerCase = false;
        options.TreatPublicMethodsAsCommands = false;
    }).ConfigureAwait(false);
