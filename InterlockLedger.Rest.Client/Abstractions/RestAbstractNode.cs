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
using System.Security.Cryptography.X509Certificates;
using System.Text;
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
        public IEnumerable<T> Chains => Get<IEnumerable<ChainIdModel>>("/chain").Select(c => BuildChain(c));
        public NodeDetailsModel Details => Get<NodeDetailsModel>("/");
        public IEnumerable<T> Mirrors => Get<IEnumerable<ChainIdModel>>("/mirrors").Select(c => BuildChain(c));
        public RestNetwork Network { get; }
        public IEnumerable<PeerModel> Peers => Get<IEnumerable<PeerModel>>("/peers");

        public IEnumerable<ChainIdModel> AddMirrorsOf(IEnumerable<string> newMirrors)
            => Post<IEnumerable<ChainIdModel>>("/mirrors", newMirrors);

        string IRestNodeInternals.CallApiPlainDoc(string url, string method, string accept)
            => CallApiPlainDoc(url, method, accept);

        RawDocumentModel IRestNodeInternals.CallApiRawDoc(string url, string method, string accept)
            => CallApiRawDoc(url, method, accept);

        public ChainCreatedModel CreateChain(ChainCreationModel model)
            => Post<ChainCreatedModel>($"/chain", model);

        TR IRestNodeInternals.Get<TR>(string url) => Get<TR>(url);

        FileInfo IRestNodeInternals.GetFile(string url, string accept, DirectoryInfo folderToStore, string method)
            => GetFile(folderToStore, url, accept, method);

        public IEnumerable<InterlockingRecordModel> InterlocksOf(string chain)
            => Get<IEnumerable<InterlockingRecordModel>>($"/interlockings/{chain}");

        TR IRestNodeInternals.Post<TR>(string url, object body) => Post<TR>(url, body);

        TR IRestNodeInternals.PostRaw<TR>(string url, byte[] body, string contentType)
            => PostRaw<TR>(url, body, contentType);

        bool IRestNodeInternals.PostStream(string url, Stream body, string contentType)
           => PostStream(url, body, contentType);

        protected readonly X509Certificate2 _certificate;

        protected static HttpWebResponse GetResponse(HttpWebRequest req) {
            return Validated(GetInnerResponse(req));

            static HttpWebResponse Validated(HttpWebResponse resp) => IfOkOrCreatedReturn(resp, resp);
            static HttpWebResponse GetInnerResponse(HttpWebRequest req) {
                try {
                    return (HttpWebResponse)req.GetResponse();
                } catch (WebException e) {
                    return (HttpWebResponse)e.Response
                        ?? throw new InvalidOperationException($"Could not retrieve response at {req.Address}", e);
                }
            }
        }

        protected static string GetStringResponse(HttpWebRequest req) => ReadAsString(GetResponse(req));

        protected static string ReadAsString(HttpWebResponse resp) {
            if (resp == null)
                return string.Empty;
            using var readStream = new StreamReader(resp.GetResponseStream());
            return readStream.ReadToEnd();
        }

        protected abstract T BuildChain(ChainIdModel c);

        protected string CallApi(string url, string method, string accept = "application/json")
            => GetStringResponse(PrepareRequest(url, method, accept));

        protected string CallApiPlainDoc(string url, string method, string accept = "plain/text")
            => GetStringResponse(PrepareRequest(url, method, accept));

        protected RawDocumentModel CallApiRawDoc(string url, string method, string accept = "*")
            => GetRawResponse(PrepareRequest(url, method, accept));

        protected void CopyFileTo(Func<string, Stream> getWritingStreamWithName, string url, string accept, string method = "GET") {
            if (getWritingStreamWithName is null)
                throw new ArgumentNullException(nameof(getWritingStreamWithName));
            using var readStream = GetFileReadStream(url, accept, method, out string name);
            var fileStream = getWritingStreamWithName(name);
            if (fileStream is null)
                throw new ArgumentNullException(nameof(fileStream));
            if (!fileStream.CanWrite)
                throw new InvalidOperationException("Can't write in the desired stream");
            readStream.CopyTo(fileStream);
            fileStream.Flush();
        }

        protected TR Get<TR>(string url) => Deserialize<TR>(CallApi(url, "GET"));

        protected FileInfo GetFile(DirectoryInfo folderToStore, string url, string accept, string method = "GET")
            => folderToStore is null
                ? throw new ArgumentNullException(nameof(folderToStore))
                : !folderToStore.Exists
                    ? throw new InvalidOperationException("Folder to store file doesn't exist")
                    : CopyFileToFolder(folderToStore, url, accept, method) ?? throw new InvalidOperationException("File details not set");

        protected TR Post<TR>(string url, object body)
            => Deserialize<TR>(GetStringResponse(PreparePostRequest(url, body, accept: "application/json")));

        protected TR PostRaw<TR>(string url, byte[] body, string contentType)
            => Deserialize<TR>(GetStringResponse(PreparePostRawRequest(url, body, accept: "application/json", contentType)));

        protected bool PostStream(string url, Stream body, string contentType)
            => IfOkOrCreatedReturn(GetResponse(PreparePostStreamRequest(url, body, accept: "*/*", contentType)), true);

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

        private static byte[] ArrayConcat(byte[] firstBuffer, byte[] secondBuffer, int count) {
            var concatBuffer = new byte[firstBuffer.Length + count];
            Array.Copy(firstBuffer, concatBuffer, firstBuffer.Length);
            Array.ConstrainedCopy(secondBuffer, 0, concatBuffer, firstBuffer.Length, count);
            return concatBuffer;
        }

        private static TR Deserialize<TR>(string s) => JsonConvert.DeserializeObject<TR>(s);

        private static X509Certificate2 GetCertFromFile(string certPath, string certPassword)
            => new X509Certificate2(certPath, certPassword, X509KeyStorageFlags.PersistKeySet);

        private static RawDocumentModel GetRawResponse(HttpWebRequest req) {
            using var resp = GetResponse(req);
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

        private static TR IfOkOrCreatedReturn<TR>(HttpWebResponse resp, TR result) => resp.StatusCode switch {
            HttpStatusCode.OK or HttpStatusCode.Created => result,
            _ => throw new InvalidDataException($"API error: {resp.StatusCode} {resp.StatusDescription}{Environment.NewLine}{ReadAsString(resp)}")
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

        private FileInfo CopyFileToFolder(DirectoryInfo folderToStore, string url, string accept, string method) {
            FileInfo fileInfo = null;
            CopyFileTo((name) => (fileInfo = GetUniquelyNamedFile(folderToStore, name)).OpenWrite(), url, accept, method);
            fileInfo?.Refresh();
            return fileInfo;

            static FileInfo GetUniquelyNamedFile(DirectoryInfo folderToStore, ReadOnlySpan<char> name) {
                var extension = Path.GetExtension(name);
                var baseName = Path.GetFileNameWithoutExtension(name);
                int i = 0;
                string suffix = string.Empty;
                FileInfo fi;
                do {
                    if (i > 100)
                        throw new InvalidOperationException("There are already a hundred numbered files with the same name");
                    var filename = new StringBuilder().Append(baseName).Append(suffix).Append(extension).ToString();
                    fi = new FileInfo(Path.Combine(folderToStore.FullName, filename));
                    suffix = $"({++i})";
                } while (fi.Exists);
                return fi;
            }
        }

        private Stream GetFileReadStream(string url, string accept, string method, out string name) {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentException($"'{nameof(url)}' cannot be null or empty", nameof(url));
            if (string.IsNullOrWhiteSpace(accept))
                throw new ArgumentException($"'{nameof(accept)}' cannot be null or empty", nameof(accept));
            if (string.IsNullOrWhiteSpace(method))
                throw new ArgumentException($"'{nameof(method)}' cannot be null or empty", nameof(method));
            var resp = GetResponse(PrepareRequest(url, method, accept));
            name = ParseFileName(resp);
            return resp.GetResponseStream();
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
        string CallApiPlainDoc(string url, string method, string accept = "text/plain");

        RawDocumentModel CallApiRawDoc(string url, string method, string accept = "*/*");

        TR Get<TR>(string url);

        FileInfo GetFile(string url, string accept, DirectoryInfo folderToStore, string method = "GET");

        TR Post<TR>(string url, object body);

        TR PostRaw<TR>(string url, byte[] body, string contentType);

        bool PostStream(string url, Stream body, string contentType);
    }
}