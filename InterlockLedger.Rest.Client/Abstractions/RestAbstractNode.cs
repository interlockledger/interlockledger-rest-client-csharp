/******************************************************************************************************************************

Copyright (c) 2018-2020 InterlockLedger Network
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
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using InterlockLedger.Rest.Client.V3;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace InterlockLedger.Rest.Client.Abstractions
{
    public abstract class RestAbstractNode<T> : IRestNodeInternals where T : RestAbstractChain
    {
        public RestAbstractNode(X509Certificate2 x509Certificate, NetworkPredefinedPorts networkId = NetworkPredefinedPorts.MainNet, string address = "localhost")
            : this(x509Certificate, (ushort)networkId, address) { }

        public RestAbstractNode(X509Certificate2 x509Certificate, ushort port, string address = "localhost") {
            _certificate = x509Certificate;
            BaseUri = new Uri($"https://{address}:{port}/", UriKind.Absolute);
            Network = new RestNetwork(this);
        }

        public RestAbstractNode(string certFile, string certPassword, NetworkPredefinedPorts networkId = NetworkPredefinedPorts.MainNet, string address = "localhost")
            : this(certFile, certPassword, (ushort)networkId, address) { }

        public RestAbstractNode(string certFile, string certPassword, ushort port, string address = "localhost")
            : this(GetCertFromFile(certFile, certPassword), port, address) { }

        public Uri BaseUri { get; }
        public string CertificateName => _certificate.FriendlyName;

        public RestNetwork Network { get; }

        public Task<IEnumerable<ChainIdModel>> AddMirrorsOfAsync(IEnumerable<string> newMirrors)
            => PostAsync<IEnumerable<ChainIdModel>>("/mirrors", newMirrors);

        Task<string> IRestNodeInternals.CallApiPlainDocAsync(string url, string method, string accept)
            => CallApiPlainDocAsync(url, method, accept);

        Task<RawDocumentModel> IRestNodeInternals.CallApiRawDocAsync(string url, string method, string accept)
            => CallApiRawDocAsync(url, method, accept);

        public Task<ChainCreatedModel> CreateChainAsync(ChainCreationModel model)
            => PostAsync<ChainCreatedModel>($"/chain", model);

        Task<TR> IRestNodeInternals.GetAsync<TR>(string url) => GetAsync<TR>(url);

        public async Task<IEnumerable<T>> GetChainsAsync() => (await GetAsync<IEnumerable<ChainIdModel>>("/chain")).Select(c => BuildChain(c));

        public Task<NodeDetailsModel> GetDetailsAsync() => GetAsync<NodeDetailsModel>("/");

        public async Task<IEnumerable<T>> GetMirrorsAsync() => (await GetAsync<IEnumerable<ChainIdModel>>("/mirrors")).Select(c => BuildChain(c));

        public Task<IEnumerable<PeerModel>> GetPeersAsync() => GetAsync<IEnumerable<PeerModel>>("/peers");

        public async Task<IEnumerable<InterlockingRecordModel>> InterlocksOfAsync(string chain)
            => await GetAsync<IEnumerable<InterlockingRecordModel>>($"/interlockings/{chain}");

        Task<TR> IRestNodeInternals.PostAsync<TR>(string url, object body) => PostAsync<TR>(url, body);

        Task<TR> IRestNodeInternals.PostRawAsync<TR>(string url, byte[] body, string contentType)
            => PostRawAsync<TR>(url, body, contentType);

        Task<bool> IRestNodeInternals.PostStreamAsync(string url, Stream body, string contentType)
            => PostStreamAsync<bool>(url, body, contentType);

        protected readonly X509Certificate2 _certificate;

        protected static async Task<HttpWebResponse> GetResponseAsync(HttpWebRequest req) {
            return await ValidatedAsync(await GetInnerResponseAsync(req).ConfigureAwait(false));

            static async Task<HttpWebResponse> ValidatedAsync(HttpWebResponse resp) => await IfOkOrCreatedReturnAsync(resp, () => Task.FromResult(resp));
            static async Task<HttpWebResponse> GetInnerResponseAsync(HttpWebRequest req) {
                try {
                    return (HttpWebResponse)await req.GetResponseAsync().ConfigureAwait(false);
                } catch (WebException e) {
                    return (HttpWebResponse)e.Response
                        ?? throw new InvalidOperationException($"Could not retrieve response at {req.Address}", e);
                }
            }
        }

        protected static async Task<string> GetStringResponseAsync(HttpWebRequest req) => await ReadAsStringAsync(await GetResponseAsync(req));

        protected static async Task<string> ReadAsStringAsync(HttpWebResponse resp) {
            if (resp == null)
                return string.Empty;
            using var readStream = new StreamReader(resp.GetResponseStream());
            return await readStream.ReadToEndAsync();
        }

        protected abstract T BuildChain(ChainIdModel c);

        protected async Task<string> CallApiAsync(string url, string method, string accept = "application/json")
            => await GetStringResponseAsync(PrepareRequest(url, method, accept));

        protected async Task<string> CallApiPlainDocAsync(string url, string method, string accept = "plain/text")
            => await GetStringResponseAsync(PrepareRequest(url, method, accept));

        protected async Task<RawDocumentModel> CallApiRawDocAsync(string url, string method, string accept = "*")
            => await GetRawResponseAsync(PrepareRequest(url, method, accept));

        protected async Task<TR> GetAsync<TR>(string url) => Deserialize<TR>(await CallApiAsync(url, "GET").ConfigureAwait(false));

        protected async Task<(string Name, string ContentType, Stream Content)> GetFileReadStreamAsync(string url, string accept = "*/*", string method = "GET") {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentException($"'{nameof(url)}' cannot be null or empty", nameof(url));
            if (string.IsNullOrWhiteSpace(accept))
                throw new ArgumentException($"'{nameof(accept)}' cannot be null or empty", nameof(accept));
            if (string.IsNullOrWhiteSpace(method))
                throw new ArgumentException($"'{nameof(method)}' cannot be null or empty", nameof(method));
            var resp = await GetResponseAsync(PrepareRequest(url, method, accept));
            return (ParseFileName(resp), resp.ContentType, resp.GetResponseStream());
        }

        protected async Task<TR> PostAsync<TR>(string url, object body)
                    => Deserialize<TR>(await GetStringResponseAsync(PreparePostRequest(url, body, accept: "application/json")));

        protected async Task<TR> PostRawAsync<TR>(string url, byte[] body, string contentType)
            => Deserialize<TR>(await GetStringResponseAsync(PreparePostRawRequest(url, body, accept: "application/json", contentType)));

        protected async Task<TR> PostStreamAsync<TR>(string url, Stream body, string contentType) {
            var resp = await GetResponseAsync(PreparePostStreamRequest(url, body, accept: "*/*", contentType));
            return await IfOkOrCreatedReturnAsync(resp, async () => Deserialize<TR>(await ReadAsStringAsync(resp)));
        }

        protected HttpWebRequest PrepareRequest(string url, string method, string accept) {
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

        protected bool ServerCertificateValidation(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
            => true;

        private static readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        private static readonly Encoding _utf8WithoutBOM = new UTF8Encoding(false);

        private static byte[] ArrayConcat(byte[] firstBuffer, Memory<byte> secondBuffer, int count) {
            var concatBuffer = new byte[firstBuffer.Length + count];
            Array.Copy(firstBuffer, concatBuffer, firstBuffer.Length);
            secondBuffer.Slice(0, count).CopyTo(concatBuffer.AsMemory(firstBuffer.Length));
            return concatBuffer;
        }

        private static async Task<TR> CheckMessageAsync<TR>(HttpWebResponse resp) {
            string content = await ReadAsStringAsync(resp).ConfigureAwait(false);
            return content.Contains("outstanding", StringComparison.OrdinalIgnoreCase)
                ? default(TR)
                : throw new InvalidDataException($"API error: {resp.StatusCode}(#{(int)resp.StatusCode}) {resp.StatusDescription}{Environment.NewLine}{content}");
        }

        private static TR Deserialize<TR>(string s) => JsonConvert.DeserializeObject<TR>(s);

        private static X509Certificate2 GetCertFromFile(string certPath, string certPassword)
            => new X509Certificate2(certPath, certPassword, X509KeyStorageFlags.PersistKeySet);

        private static async Task<RawDocumentModel> GetRawResponseAsync(HttpWebRequest req) {
            var response = await GetResponseAsync(req);
            if (response is null)
                return null;
            using var resp = response;
            using var readStream = resp.GetResponseStream();
            var fullBuffer = Array.Empty<byte>();
            var buffer = new byte[0x80000].AsMemory();
            while (true) {
                var readCount = await readStream.ReadAsync(buffer, cancellationToken: CancellationToken.None);
                if (readCount == 0)
                    break;
                fullBuffer = ArrayConcat(fullBuffer, buffer, readCount);
            }
            return new RawDocumentModel(resp.ContentType, fullBuffer, ParseFileName(resp));
        }

        private static async Task<TR> IfOkOrCreatedReturnAsync<TR>(HttpWebResponse resp, Func<Task<TR>> buildResult) => resp.StatusCode switch
        {
            HttpStatusCode.OK or HttpStatusCode.Created => await buildResult(),
            HttpStatusCode.NotFound => default,
            HttpStatusCode.Unauthorized => throw new SecurityException(nameof(HttpStatusCode.Unauthorized)),
            HttpStatusCode.Forbidden => throw new SecurityException(nameof(HttpStatusCode.Forbidden)),
            _ => await CheckMessageAsync<TR>(resp)
        };

        private static string ParseFileName(HttpWebResponse resp) {
            var disposition = resp.GetResponseHeader("Content-Disposition");
            var header = new ContentDisposition(disposition);
            var filename = header.FileName;
            if (header.Parameters.ContainsKey("filename*")) {
                filename = header.Parameters["filename*"];
                if (filename.StartsWith("UTF-8''"))
                    return WebUtility.UrlDecode(filename[7..]);
            }
            return filename;
        }

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

        private HttpWebRequest PreparePostStreamRequest(string url, Stream body, string accept, string contentType) {
            var request = PrepareRequest(url, "POST", accept);
            request.ContentType = contentType;
            using (var stream = request.GetRequestStream()) {
                body.CopyTo(stream);
                stream.Flush();
            }
            return request;
        }
    }

    internal interface IRestNodeInternals
    {
        Task<string> CallApiPlainDocAsync(string url, string method, string accept = "text/plain");

        Task<RawDocumentModel> CallApiRawDocAsync(string url, string method, string accept = "*/*");

        Task<TR> GetAsync<TR>(string url);

        Task<TR> PostAsync<TR>(string url, object body);

        Task<TR> PostRawAsync<TR>(string url, byte[] body, string contentType);

        Task<bool> PostStreamAsync(string url, Stream body, string contentType);
    }
}