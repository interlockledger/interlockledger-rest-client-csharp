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

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace InterlockLedger.Rest.Client;

[JsonConverter(typeof(Converter))]
[TypeConverter(typeof(TConverter))]
public class ConnectionString : IParsable<ConnectionString>
{

    public sealed class Converter : JsonConverter<ConnectionString>
    {
        public override ConnectionString? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => TryParse(reader.GetString());

        public override void Write(Utf8JsonWriter writer, ConnectionString value, JsonSerializerOptions options) => writer.WriteStringValue(value.ToString());
    }
    public sealed class TConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) => sourceType == typeof(string);
        public override bool CanConvertTo(ITypeDescriptorContext? context, [NotNullWhen(true)] Type? destinationType) => destinationType == typeof(string);
        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value) => TryParse(value as string);
        public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType) => value?.ToString();

    }

    public KnownNetworks Network { get; private set; }
    public string Host { get; private set; }
    public ushort Port { get; private set; }
    public string ClientCertificateFilePath { get; private set; }
    public string ClientCertificatePassword { get; private set; }
    public override string ToString() => _textualRepresentation;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public ConnectionString(string? connectionString) {
        if (!InitializedFrom(connectionString))
            throw new InvalidOperationException($"Could not build ConnectionString from: {connectionString}");
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.


    /// <summary>
    /// Parses a connection string to InterlockLedger REST Client ConnectionString
    /// </summary>
    /// <param name="connectionString">Connection string in the form "https://minerva-data.il2.io[:32067],/absolute/path/to/clientcertificate.pfx,clientCertificatePassword[,minerva]"</param>
    /// <returns>Parsed <see cref="ConnectionString"/></returns>
    /// <exception cref="ArgumentException"></exception>
    public static ConnectionString Parse(string connectionString) => new(connectionString);

    /// <summary>
    /// Parses a connection string to InterlockLedger REST Client ConnectionString
    /// </summary>
    /// <param name="connectionString">Connection string in the form "https://minerva-data.il2.io[:32067],/absolute/path/to/clientcertificate.pfx,clientCertificatePassword[,minerva]"</param>
    /// <param name="provider">[Ignored] Format provider</param>
    /// <returns>Parsed <see cref="ConnectionString"/></returns>
    /// <exception cref="ArgumentException"></exception>
    public static ConnectionString Parse(string connectionString, IFormatProvider? provider) => new(connectionString);

    /// <summary>
    /// Try to parse a connection string to InterlockLedger REST Client ConnectionString
    /// </summary>
    /// <param name="connectionString">Connection string in the form "https://minerva-data.il2.io[:32067],/absolute/path/to/clientcertificate.pfx,clientCertificatePassword[,minerva]"</param>
    /// <param name="provider">[Ignored] Format provider</param>
    /// <param name="result">Parsed <see cref="ConnectionString"/> if true</param>
    /// <returns>True if correctly parsed</returns>
    public static bool TryParse([NotNullWhen(true)] string? connectionString, IFormatProvider? provider, [MaybeNullWhen(false)] out ConnectionString result) {
        result = new ConnectionString();
        return result.InitializedFrom(connectionString);
    }

    private static ConnectionString? TryParse(string? value) => TryParse(value, null, out var result) ? result : null;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private ConnectionString() {
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    private string _textualRepresentation;
    private bool InitializedFrom([NotNullWhen(true)] string? connectionString) {
        if (!string.IsNullOrWhiteSpace(connectionString)) {
            var match = ConfigurationOptionsParsingHelper.ConnectionStringRegex().Match(connectionString);
            var groups = match.Groups;
            if (match.Success && groups.Count >= 8 && ParsePort(groups[3], groups[7], out var network, out ushort port)) {
                try {
                    string host = groups[1].Required("Host");
                    string clientCertificateFilePath = groups[4].Required("CertificateFilePath");
                    string clientCertificatePassword = groups[5].Required("CertificatePassword");
                    InitializedWith(network, host, port, clientCertificateFilePath, clientCertificatePassword);
                    return true;
                } catch (Exception e) {
                    _textualRepresentation = $"[ERROR] {e.Message}";
                }
            }
        }
        return false;

        void InitializedWith(KnownNetworks network, string host, ushort port, string clientCertificateFilePath, string clientCertificatePassword) {
            Network = network;
            Host = host.Required().ToLowerInvariant();
            Port = port == 0 ? network.DefaultPort : port;
            ClientCertificateFilePath = clientCertificateFilePath.Required();
            ClientCertificatePassword = clientCertificatePassword.Required();
            _textualRepresentation = $"https://{Host}:{Port},{ClientCertificateFilePath},{ClientCertificatePassword},{Network}";
        }

        static bool ParsePort(Group groupPort, Group groupNetwork, out KnownNetworks network, out ushort port) {
            network = new(groupNetwork.WithDefault("MainNet"));
            port = network.DefaultPort;
            return !groupPort.Success || ushort.TryParse(groupPort.Value, CultureInfo.InvariantCulture, out port);
        }
    }


}

public static class GroupExtensions
{
    public static string WithDefault(this Group group, string @default) => group.Success ? group.Value : @default;
    public static string Required(this Group group, string name) => group.Success ? group.Value : throw ObjectExtensions.ArgRequired(name);

}

internal static partial class ConfigurationOptionsParsingHelper
{
    [GeneratedRegex(@"^https://([^:,]+)(:(\d+))?,([^,]+),([^,]+)(,(\w+))?$")]
    public static partial Regex ConnectionStringRegex();
}
