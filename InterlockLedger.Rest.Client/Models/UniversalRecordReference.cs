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

/// <summary>
/// Universal reference to a record
/// </summary>
/// <param name="network">Network name where the chain is located</param>
/// <param name="chainId"><inheritdoc/></param>
/// <param name="serial"><inheritdoc/></param>
[JsonConverter(typeof(JsonConverterFor<UniversalRecordReference>))]
public class UniversalRecordReference(string network, string chainId, ulong serial) : RecordReference(chainId, serial), IParsable<UniversalRecordReference>
{
    public  string Network { get;  } = network;

    private UniversalRecordReference(string network, RecordReference recordReference) : this(network, recordReference.ChainId, recordReference.Serial) { }
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out UniversalRecordReference result) {
        if (!string.IsNullOrWhiteSpace(s)) {
            var parts = s.Split(_separator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length == 2 && RecordReference.TryParse(parts[1], provider, out var recordReference)) {
                result = new UniversalRecordReference(parts[0], recordReference);
                return true;
            }
        }
        result = null;
        return false;
    }
    static UniversalRecordReference IParsable<UniversalRecordReference>.Parse(string s, IFormatProvider? provider) =>
        TryParse(s, provider, out var result) ? result : throw new FormatException($"Invalid string '{s}' to parse as an UniversalRecordReference");

    private const char _separator = ':';

}
