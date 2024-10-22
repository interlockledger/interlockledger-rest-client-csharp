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

using System.Diagnostics.CodeAnalysis;

namespace InterlockLedger.Rest.Client;

[JsonConverter(typeof(Converter))]
public class ConnectionString : IParsable<ConnectionString>
{
    public sealed class Converter : JsonConverter<ConnectionString>
    {
        public override ConnectionString? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => Parse(reader.GetString().Required());
        public override void Write(Utf8JsonWriter writer, ConnectionString value, JsonSerializerOptions options) => writer.WriteStringValue(value.ToString());
    }

    public KnownNetworks Network { get; set; }
    public string Host { get; set; }
    public ushort Port { get; set; }
    public string? ClientCertificateFilePath { get; set; }
    public string? ClientCertificatePassword { get; set; }

    public ConnectionString(string host, ushort? port = null, string? clientCertificateFilePath = null, string? clientCertificatePassword = null, string? network = null) {
        Network = new(network);
        Host = host.Required().ToLowerInvariant();
        Port = port.GetValueOrDefault(Network.DefaultPort);
        ClientCertificateFilePath = clientCertificateFilePath;
        ClientCertificatePassword = clientCertificatePassword;
    }

    public override string ToString() {
        var sb =
            new StringBuilder("ilkl-")
            .Append(Network)
            .Append("://")
            .Append(Host)
            .Append(':')
            .Append(Port);
        if (!ClientCertificateFilePath.IsBlank()) {
            sb.Append(',').Append(ClientCertificateFilePath);
            if (!ClientCertificatePassword.IsBlank()) {
                sb.Append(',').Append(ClientCertificatePassword);
            }
        }
        return sb.ToString();
    }

    /// <summary>
    /// Parses a connection string to InterlockLedger REST Client ConnectionString
    /// </summary>
    /// <param name="connectionString">Connection string in the form "ilkl-minerva://minerva-data.il2.io[:32067],/absolute/path/to/clientcertificate.pfx,clientCertificatePassword"</param>
    /// <returns>Parsed ConnectionString</returns>
    /// <exception cref="ArgumentException"></exception>
    public static ConnectionString Parse(string connectionString) => Parse(connectionString, null);

    /// <summary>
    /// Parses a connection string to InterlockLedger REST Client ConnectionString
    /// </summary>
    /// <param name="connectionString">Connection string in the form "ilkl-minerva://minerva-data.il2.io[:32067],/absolute/path/to/clientcertificate.pfx,clientCertificatePassword"</param>
    /// <param name="provider">[Ignored] Format provider</param>
    /// <returns>Parsed ConnectionString</returns>
    /// <exception cref="ArgumentException"></exception>
    public static ConnectionString Parse(string connectionString, IFormatProvider? provider) =>
        TryParse(connectionString, provider, out var result)
            ? result
            : throw new ArgumentException("Malformed connection string", nameof(connectionString));

    /// <summary>
    /// Try to parse a connection string to InterlockLedger REST Client ConnectionString
    /// </summary>
    /// <param name="connectionString">Connection string in the form "ilkl-minerva://minerva-data.il2.io[:32067],/absolute/path/to/clientcertificate.pfx,clientCertificatePassword"</param>
    /// <param name="provider"></param>
    /// <param name="result"></param>
    /// <returns>True if correctly parsed</returns>
    public static bool TryParse([NotNullWhen(true)] string? connectionString, IFormatProvider? provider, [MaybeNullWhen(false)] out ConnectionString result) {
        if (!string.IsNullOrWhiteSpace(connectionString)) {
            var match = ConfigurationOptionsParsingHelper.ConnectionStringRegex().Match(connectionString);
            var groups = match.Groups;
            if (match.Success && groups.Count >= 8 && ParsePort(groups[4], groups[1].Value, out ushort port)) {
                result = new ConnectionString(groups[2].Value, port, GetValueOrDefault(groups[6]), GetValueOrDefault(groups[7]), groups[1].Value);
                return true;
            }
        }
        result = null;
        return false;
    }
    private static string? GetValueOrDefault(Group group) => group.Success ? group.Value : null;
    private static bool ParsePort(Group group, string network, out ushort port) {
        if (group.Success)
            return ushort.TryParse(group.Value, null, out port);
        port = new KnownNetworks(network).DefaultPort;
        return true;
    }

}

internal static partial class ConfigurationOptionsParsingHelper
{
    [GeneratedRegex(@"^ilkl-(\w+)://([^:,]+)(:(\d+))?(,([^,]+),([^,]+))?$")]
    public static partial Regex ConnectionStringRegex();
}
