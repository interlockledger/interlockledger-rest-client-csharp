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

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mime;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace InterlockLedger
{
    public class RestClient
    {
        public RestClient(string certFile, string certPassword, NetworkPredefinedPorts networkId = NetworkPredefinedPorts.MainNet, string address = "localhost")
            : this(certFile, certPassword, (ushort)networkId, address) { }

        public RestClient(string certFile, string certPassword, ushort port, string address = "localhost") {
            BaseUri = new Uri($"https://{address}:{port}/", UriKind.Absolute);
            _certificate = GetCertFromFile(certFile, certPassword);
        }

        public AppsModel Apps => Get<AppsModel>("/apps");
        public Uri BaseUri { get; }
        public string CertificateName => _certificate.FriendlyName;
        public IEnumerable<ChainIdModel> Chains => Get<IEnumerable<ChainIdModel>>("/chain");
        public IEnumerable<ChainIdModel> Mirrors => Get<IEnumerable<ChainIdModel>>("/mirrors");
        public NodeDetailsModel NodeDetails => Get<NodeDetailsModel>("/");
        public IEnumerable<PeerModel> Peers => Get<IEnumerable<PeerModel>>("/peers");

        public IEnumerable<ulong> ActiveAppsOn(string chain) => Get<IEnumerable<ulong>>($"/chain/{chain}/activeApps");

        public IEnumerable<ChainIdModel> AddMirrorOf(IEnumerable<string> newMirrors) => Post<IEnumerable<ChainIdModel>>("/mirrors", newMirrors);

        public IEnumerable<DocumentDetailsModel> DocumentsOn(string chain) => Get<IEnumerable<DocumentDetailsModel>>($"/chain/{chain}/document");

        public string GetPlainDocument(string chain, string fileId) => CallPlainDocApi($"/chain/{chain}/document/{fileId}", "GET");

        public RawDocumentModel GetRawDocument(string chain, string fileId) => CallRawDocApi($"/chain/{chain}/document/{fileId}", "GET");

        public IEnumerable<InterlockingRecordModel> InterlocksOf(string chain) => Get<IEnumerable<InterlockingRecordModel>>($"/interlockings/{chain}");

        public IEnumerable<InterlockingRecordModel> InterlocksStoredOn(string chain) => Get<IEnumerable<InterlockingRecordModel>>($"/chain/{chain}/interlock");

        public IEnumerable<KeyModel> PermittedKeysOn(string chain) => Get<IEnumerable<KeyModel>>($"/chain/{chain}/key");

        public IEnumerable<RecordModel> Records(string chain) => Get<IEnumerable<RecordModel>>($"/chain/{chain}/record");

        public IEnumerable<RecordModel> Records(string chain, ulong firstSerial) => Get<IEnumerable<RecordModel>>($"/chain/{chain}/record?firstSerial={firstSerial}");

        public IEnumerable<RecordModel> Records(string chain, ulong firstSerial, ulong lastSerial) => Get<IEnumerable<RecordModel>>($"/chain/{chain}/record?firstSerial={firstSerial}&lastSerial={lastSerial}");

        private readonly X509Certificate2 _certificate;

        private static byte[] ArrayConcat(byte[] firstBuffer, byte[] secondBuffer, int count) {
            var concatBuffer = new byte[firstBuffer.Length + count];
            Array.Copy(firstBuffer, concatBuffer, firstBuffer.Length);
            Array.ConstrainedCopy(secondBuffer, 0, concatBuffer, firstBuffer.Length, count);
            return concatBuffer;
        }

        private static T Deserialize<T>(string s) => JsonConvert.DeserializeObject<T>(s);

        private static X509Certificate2 GetCertFromFile(string certPath, string certPassword)
            => new X509Certificate2(certPath, certPassword, X509KeyStorageFlags.PersistKeySet);

        private static HttpWebResponse GetResponse(HttpWebRequest req) {
            var resp = (HttpWebResponse)req.GetResponse();
            if (resp.StatusCode != HttpStatusCode.OK)
                throw new InvalidDataException($"API error: {resp.StatusCode} {resp.StatusDescription}");
            return resp;
        }

        private static string GetStringResponse(HttpWebRequest req) {
            HttpWebResponse resp = GetResponse(req);
            using (var readStream = new StreamReader(resp.GetResponseStream())) {
                return readStream.ReadToEnd();
            }
        }

        private static string ParseFileName(HttpWebResponse resp) {
            var disposition = resp.GetResponseHeader("Content-Disposition");
            var header = new ContentDisposition(disposition);
            var filename = header.FileName;
            if (header.Parameters.ContainsKey("filename*")) {
                filename = header.Parameters["filename*"];
                if (filename.StartsWith("UTF-8''"))
                    return WebUtility.UrlDecode(filename.Substring(7));
            }
            return filename;
        }

        private string CallApi(string url, string method, string accept = "application/json")
            => GetStringResponse(PrepareRequest(url, method, accept));

        private string CallPlainDocApi(string url, string method, string accept = "plain/text")
            => GetStringResponse(PrepareRequest(url, method, accept));

        private RawDocumentModel CallRawDocApi(string url, string method, string accept = "*")
            => GetRawResponse(PrepareRequest(url, method, accept));

        private T Get<T>(string url) => Deserialize<T>(CallApi(url, "GET"));

        private RawDocumentModel GetRawResponse(HttpWebRequest req) {
            HttpWebResponse resp = GetResponse(req);
            using (var readStream = resp.GetResponseStream()) {
                var fullBuffer = new byte[0];
                var buffer = new byte[0x80000];
                do {
                    var readCount = readStream.Read(buffer, 0, buffer.Length);
                    if (readCount == 0)
                        break;
                    fullBuffer = ArrayConcat(fullBuffer, buffer, readCount);
                } while (true);
                return new RawDocumentModel(resp.ContentType, fullBuffer, ParseFileName(resp));
            }
        }

        private T Post<T>(string url, object body) => Deserialize<T>(GetStringResponse(PreparePostRequest(url, body)));

        private HttpWebRequest PreparePostRequest(string url, object body) {
            var request = PrepareRequest(url, "POST", "application/json");
            request.ContentType = "application/json";
            using (var stream = request.GetRequestStream()) {
                var json = JsonConvert.SerializeObject(body);
                var writer = new StreamWriter(stream, Encoding.UTF8);
                writer.Write(json);
                writer.Flush();
            }
            return request;
        }

        private HttpWebRequest PrepareRequest(string url, string method, string accept) {
            var req = (HttpWebRequest)WebRequest.Create(new Uri(BaseUri, url));
            req.AllowAutoRedirect = false;
            req.AuthenticationLevel = AuthenticationLevel.MutualAuthRequired;
            req.ClientCertificates.Add(_certificate);
            req.Method = method;
            req.Accept = accept;
            req.UserAgent = nameof(InterlockLedger);
            return req;
        }
    }
}