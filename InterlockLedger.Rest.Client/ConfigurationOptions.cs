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

public class ConfigurationOptions(string host, ushort port, string? clientCertificateFilePath = null, string? clientCertificatePassword = null) : IParsable<ConfigurationOptions>
{
    public string Host { get; set; } = host;
    public ushort Port { get; set; } = port;
    public string? ClientCertificateFilePath { get; set; } = clientCertificateFilePath;
    public string? ClientCertificatePassword { get; set; } = clientCertificatePassword;

    /// <summary>
    /// Parses a connection string to InterlockLedger REST Client ConfigurationOptions
    /// </summary>
    /// <param name="connectionString">Connection string in the form "ilkl-minerva://minerva-data.il2.io[:32067],/absolute/path/to/clientcertificate.pfx,clientCertificatePassword"</param>
    /// <returns>Parsed ConfigurationOptions</returns>
    /// <exception cref="ArgumentException"></exception>
    public static ConfigurationOptions Parse(string connectionString) => Parse(connectionString, null);

    /// <summary>
    /// Parses a connection string to InterlockLedger REST Client ConfigurationOptions
    /// </summary>
    /// <param name="connectionString">Connection string in the form "ilkl-minerva://minerva-data.il2.io[:32067],/absolute/path/to/clientcertificate.pfx,clientCertificatePassword"</param>
    /// <param name="provider">[Ignored] Format provider</param>
    /// <returns>Parsed ConfigurationOptions</returns>
    /// <exception cref="ArgumentException"></exception>
    public static ConfigurationOptions Parse(string connectionString, IFormatProvider? provider) =>
        TryParse(connectionString, provider, out var result)
            ? result
            : throw new ArgumentException("Malformed connection string", nameof(connectionString));

    /// <summary>
    /// Try to parse a connection string to InterlockLedger REST Client ConfigurationOptions
    /// </summary>
    /// <param name="connectionString">Connection string in the form "ilkl-minerva://minerva-data.il2.io[:32067],/absolute/path/to/clientcertificate.pfx,clientCertificatePassword"</param>
    /// <param name="provider"></param>
    /// <param name="result"></param>
    /// <returns>True if correctly parsed</returns>
    public static bool TryParse([NotNullWhen(true)] string? connectionString, IFormatProvider? provider, [MaybeNullWhen(false)] out ConfigurationOptions result) {
        if (!string.IsNullOrWhiteSpace(connectionString)) {
            var match = ConfigurationOptionsParsingHelper.ConnectionStringRegex().Match(connectionString);
            var groups = match.Groups;
            if (match.Success && groups.Count >= 8 && ParsePort(groups[4], groups[1].Value, out ushort port)) {
                result = new ConfigurationOptions(groups[2].Value, port, GetValueOrDefault(groups[6]), GetValueOrDefault(groups[7]));
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
        port = ConfigurationOptionsParsingHelper.GetDefaultPortFor(network);
        return true;
    }
}

internal static partial class ConfigurationOptionsParsingHelper
{
    [GeneratedRegex(@"^ilkl-(\w+)://([^:,]+)(:(\d+))?(,([^,]+),([^,]+))?$")]
    public static partial Regex ConnectionStringRegex();
    public static ushort GetDefaultPortFor(string network) => 0;
}
