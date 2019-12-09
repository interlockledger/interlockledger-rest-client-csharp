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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace InterlockLedger.Rest.Client.V2
{
    public class RestNode
    {
        public RestNode(string certFile, string certPassword, NetworkPredefinedPorts networkId = NetworkPredefinedPorts.MainNet, string address = "localhost")
            : this(certFile, certPassword, (ushort)networkId, address) { }

        public RestNode(string certFile, string certPassword, ushort port, string address = "localhost") {
            BaseUri = new Uri($"https://{address}:{port}/", UriKind.Absolute);
            _certificate = GetCertFromFile(certFile, certPassword);
            Network = new RestNetwork(this);
        }

        public Uri BaseUri { get; }
        public string CertificateName => _certificate.FriendlyName;
        public IEnumerable<RestChain> Chains => Get<IEnumerable<ChainIdModel>>("/chain").Select(c => new RestChain(this, c));
        public NodeDetailsModel Details => Get<NodeDetailsModel>("/");
        public IEnumerable<RestChain> Mirrors => Get<IEnumerable<ChainIdModel>>("/mirrors").Select(c => new RestChain(this, c));
        public RestNetwork Network { get; }

        public IEnumerable<PeerModel> Peers => Get<IEnumerable<PeerModel>>("/peers");

        public IEnumerable<ChainIdModel> AddMirrorsOf(IEnumerable<string> newMirrors)
            => Post<IEnumerable<ChainIdModel>>("/mirrors", newMirrors);

        public ChainCreatedModel CreateChain(ChainCreationModel model)
            => Post<ChainCreatedModel>($"/chain", model);

        public IEnumerable<InterlockingRecordModel> InterlocksOf(string chain)
            => Get<IEnumerable<InterlockingRecordModel>>($"/interlockings/{chain}");

        internal string CallApiPlainDoc(string url, string method, string accept = "plain/text")
            => GetStringResponse(PrepareRequest(url, method, accept));

        internal RawDocumentModel CallApiRawDoc(string url, string method, string accept = "*")
            => GetRawResponse(PrepareRequest(url, method, accept));

        internal T Get<T>(string url) => Deserialize<T>(CallApi(url, "GET"));

        internal T Post<T>(string url, object body) => Deserialize<T>(GetStringResponse(PreparePostRequest(url, body, "application/json")));

        internal T PostRaw<T>(string url, byte[] body, string contentType) => Deserialize<T>(GetStringResponse(PreparePostRawRequest(url, body, "application/json", contentType)));

        private static readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        private static readonly Encoding _utf8WithoutBOM = new UTF8Encoding(false);

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

        private static RawDocumentModel GetRawResponse(HttpWebRequest req) {
            HttpWebResponse resp = GetResponse(req);
            using var readStream = resp.GetResponseStream();
            var fullBuffer = Array.Empty<byte>();
            var buffer = new byte[0x80000];
            while (true) {
                var readCount = readStream.Read(buffer, 0, buffer.Length);
                if (readCount == 0)
                    break;
                fullBuffer = ArrayConcat(fullBuffer, buffer, readCount);
            }
            return new RawDocumentModel(resp.ContentType, fullBuffer, ParseFileName(resp));
        }

        private static HttpWebResponse GetResponse(HttpWebRequest req) {
            WebResponse GetInnerResponse() {
                try {
                    return req.GetResponse();
                } catch (WebException e) {
                    return e.Response ?? throw new InvalidOperationException($"Could not retrieve response at {req.Address}", e);
                }
            }
            var resp = (HttpWebResponse)GetInnerResponse();
            if (resp.StatusCode != HttpStatusCode.OK && resp.StatusCode != HttpStatusCode.Created)
                throw new InvalidDataException($"API error: {resp.StatusCode} {resp.StatusDescription}{Environment.NewLine}{ReadAsString(resp)}");
            return resp;
        }

        private static string GetStringResponse(HttpWebRequest req) => ReadAsString(GetResponse(req));

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

        private static string ReadAsString(HttpWebResponse resp) {
            if (resp == null)
                return string.Empty;
            using var readStream = new StreamReader(resp.GetResponseStream());
            return readStream.ReadToEnd();
        }

        private string CallApi(string url, string method, string accept = "application/json")
            => GetStringResponse(PrepareRequest(url, method, accept));

        private HttpWebRequest PreparePostRawRequest(string url, byte[] body, string accept, string contentType) {
            var request = PrepareRequest(url, "POST", accept);
            request.ContentType = contentType;
            using (var stream = request.GetRequestStream()) {
                stream.Write(body, 0, body.Length);
                stream.Flush();
            }
            return request;
        }

        private HttpWebRequest PreparePostRequest(string url, object body, string accept) {
            var request = PrepareRequest(url, "POST", accept);
            request.ContentType = "application/json; charset=utf-8";
            using (var stream = request.GetRequestStream()) {
                var json = JsonConvert.SerializeObject(body, _jsonSettings);
                using var writer = new StreamWriter(stream, _utf8WithoutBOM);
                writer.Write(json);
                writer.Flush();
            }
            return request;
        }

        private HttpWebRequest PrepareRequest(string url, string method, string accept) {
            var req = (HttpWebRequest)WebRequest.Create(new Uri(BaseUri, url));
            req.AllowAutoRedirect = false;
            req.AuthenticationLevel = AuthenticationLevel.MutualAuthRequired;
            req.ServerCertificateValidationCallback = ServerCertificateValidation;
            req.ClientCertificates.Add(_certificate);
            req.Method = method;
            req.Accept = accept;
            req.UserAgent = nameof(InterlockLedger);
            return req;
        }

        private bool ServerCertificateValidation(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
            => true;
    }
}