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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace InterlockLedger.Rest.Client
{
    [JsonConverter(typeof(Converter))]
    public class AppPermissions
    {
        public static readonly Regex Mask = new Regex("^#[0-9]+(,[0-9]+)*$");

        public AppPermissions(ulong app, params ulong[] appActions) : this(app, (IEnumerable<ulong>)appActions) {
        }

        public AppPermissions(ulong app, IEnumerable<ulong> appActions) {
            AppId = app;
            ActionIds = appActions ?? Array.Empty<ulong>();
        }

        /// <summary>
        /// App actions to be permitted by number
        /// </summary>
        public IEnumerable<ulong> ActionIds { get; set; }

        /// <summary>
        /// App to be permitted (by number)
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include, Required = Required.Always)]
        public ulong AppId { get; set; }

        public IEnumerable<AppPermissions> ToEnumerable() => new AppPermissions[] { this };

        public override string ToString() {
            var actions = ActionIds?.ToArray() ?? Array.Empty<ulong>();
            var plural = (actions.Length == 1 ? "" : "s");
            return $"App #{AppId} {(actions.Length > 0 ? $"Action{plural} {actions.WithCommas(noSpaces: true)}" : "All Actions")}";
        }

        public class Converter : JsonConverter<AppPermissions>
        {
            public override AppPermissions ReadJson(JsonReader reader, Type objectType, [AllowNull] AppPermissions existingValue, bool hasExistingValue, JsonSerializer serializer)
                => reader.TokenType switch
                {
                    JsonToken.Null => null,
                    JsonToken.String => new AppPermissions(reader.Value as string),
                    _ => throw new InvalidCastException($"TokenType should be Null or String but is {reader.TokenType}")
                };

            public override void WriteJson(JsonWriter writer, [AllowNull] AppPermissions value, JsonSerializer serializer) {
                if (value is null)
                    writer.WriteNull();
                else
                    writer.WriteValue(value.TextualRepresentation);
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

        internal string TextualRepresentation => $"#{AppId}{_firstComma}{ActionIds.WithCommas(noSpaces: true)}";

        private string _firstComma => ActionIds.Any() ? "," : string.Empty;

        private static ulong AsUlong(string s) => ulong.TryParse(s?.Trim(), out ulong result) ? result : 0;
    }
}
