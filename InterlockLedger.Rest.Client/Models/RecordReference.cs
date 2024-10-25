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
/// Network-local reference to a record
/// </summary>
/// <param name="chainId">Id of the chain containing the record</param>
/// <param name="serial">Serial numbr of the record in the chain (zero-based)</param>
[JsonConverter(typeof(JsonConverterFor<RecordReference>))]
public class RecordReference(string chainId, ulong serial) : IParsable<RecordReference>
{
    public static RecordReference Parse(string s, IFormatProvider? provider) => TryParse(s, provider, out var result) ? result : throw new FormatException($"Invalid string '{s}' to parse as a RecordReference");
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out RecordReference result) {
        if (!string.IsNullOrWhiteSpace(s)) {
            var parts = s.Split(_separator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length == 2 && ulong.TryParse(parts[1], provider, out ulong serial)) {
                result = new RecordReference(parts[0], serial);
                return true;
            }
        }
        result = null;
        return false;
    }
    public override string ToString() => $"{ChainId}{_separator}{Serial:0}";
    private const char _separator = '@';

    public  string ChainId { get; } = chainId;
    public ulong Serial { get;  } = serial;
}
