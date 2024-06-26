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
public partial class AppPermissions
{
    public static readonly Regex Mask = AppPermissionsRegex();

    public AppPermissions(ulong app, params ulong[] appActions) : this(app, (IEnumerable<ulong>)appActions) {
    }

    public AppPermissions(ulong app, IEnumerable<ulong> appActions) {
        AppId = app;
        ActionIds = appActions ?? [];
    }

    /// <summary>
    /// App actions to be permitted by number
    /// </summary>
    public IEnumerable<ulong> ActionIds { get; set; }

    /// <summary>
    /// App to be permitted (by number)
    /// </summary>
    public ulong AppId { get; set; }

    public string TextualRepresentation => $"#{AppId}{_firstComma}{ActionIds.WithCommas(noSpaces: true)}";

    public IEnumerable<AppPermissions> ToEnumerable() => [this];

    public override string ToString() {
        var actions = ActionIds?.ToArray() ?? [];
        var plural = (actions.Length == 1 ? "" : "s");
        return $"App #{AppId} {(actions.Length > 0 ? $"Action{plural} {actions.WithCommas(noSpaces: true)}" : "All Actions")}";
    }

    public class Converter : JsonConverter<AppPermissions>
    {
        public override AppPermissions? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => reader.TokenType switch {
                JsonTokenType.Null => null,
                JsonTokenType.String => new AppPermissions(reader.GetString().Required()),
                _ => throw new InvalidCastException($"TokenType should be Null or String but is {reader.TokenType}")
            };

        public override void Write(Utf8JsonWriter writer, AppPermissions value, JsonSerializerOptions options) {
            if (value is null)
                writer.WriteNullValue();
            else
                writer.WriteStringValue(value.TextualRepresentation);
        }
    }

    internal AppPermissions(string textualRepresentation) {
        if (string.IsNullOrWhiteSpace(textualRepresentation) || !Mask.IsMatch(textualRepresentation)) {
            throw new ArgumentException("Invalid textual representation '" + textualRepresentation + "'", nameof(textualRepresentation));
        }
        var source = textualRepresentation[1..].Split(',').Select(AsUlong);
        AppId = source.First();
        ActionIds = source.Skip(1).ToArray();
    }

    private string _firstComma => ActionIds.Any() ? "," : string.Empty;

    private static ulong AsUlong(string s) => ulong.TryParse(s?.Trim(), out ulong result) ? result : 0;

    [GeneratedRegex("^#[0-9]+(,[0-9]+)*$", RegexOptions.None, 100)]
    private static partial Regex AppPermissionsRegex();
}