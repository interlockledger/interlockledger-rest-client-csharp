/******************************************************************************************************************************

Copyright (c) 2018-2019 InterlockLedger Network
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
using System.Linq;
using Newtonsoft.Json;

namespace InterlockLedger.Rest.Client.V3
{
    public class AppPermissions
    {
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

        public override string ToString() {
            var actions = ActionIds?.ToArray() ?? Array.Empty<ulong>();
            var plural = (actions.Length == 1 ? "" : "s");
            return $"App #{AppId} {(actions.Length > 0 ? $"Action{plural} {actions.WithCommas(noSpaces: true)}" : "All Actions")}";
        }
    }
}