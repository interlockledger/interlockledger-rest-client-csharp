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

namespace InterlockLedger.Rest.Client;

[JsonConverter(typeof(Converter))]
public sealed class KnownNetworks
{
    public sealed class Converter : JsonConverter<KnownNetworks>
    {
        public override KnownNetworks? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => new(reader.GetString());
        public override void Write(Utf8JsonWriter writer, KnownNetworks value, JsonSerializerOptions options) => writer.WriteStringValue(value.TextualRepresentation);
    }
    public KnownNetworks() : this(NetworkPredefinedPorts.MainNet) { }
    public KnownNetworks(string? value) : this(Enum.TryParse(value, ignoreCase: true, out NetworkPredefinedPorts network) ? network : NetworkPredefinedPorts.MainNet) { }

    private KnownNetworks(NetworkPredefinedPorts network) {
        Network = network;
        TextualRepresentation = Enum.GetName(Network).Required().ToLowerInvariant();
        DefaultPort = (ushort)Network;
    }
    public ushort DefaultPort { get; }
    public NetworkPredefinedPorts Network { get; }
    public string TextualRepresentation { get; }
    public override string ToString() => TextualRepresentation;
}
