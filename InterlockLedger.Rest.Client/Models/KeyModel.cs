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

namespace InterlockLedger.Rest.Client
{
    /// <summary>
    /// Key
    /// </summary>
    public class KeyModel
    {
        public bool Actionable => Purposes.Contains("Action");

        /// <summary>
        /// App to be permitted (by number)
        /// </summary>
        public ulong App { get; set; }

        /// <summary>
        /// App actions to be permitted by number
        /// </summary>
        public IEnumerable<ulong> AppActions { get; set; }

        /// <summary>
        /// Unique key id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Key name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Key public key
        /// </summary>
        public string PublicKey { get; set; }

        /// <summary>
        /// Key valid purposes
        /// </summary>
        public IEnumerable<string> Purposes { get; set; }

        public override string ToString() => $"Key '{Name}' {Id} purposes: [{DisplayablePurposes}]  {ActionsFor.ToLowerInvariant()}";

        private string ActionsFor => Actionable ? AppAndActions() : string.Empty;
        private string DisplayablePurposes => Purposes.OrderBy(p => p).WithCommas();

        private string AppAndActions() {
            var actions = AppActions?.ToArray() ?? Array.Empty<ulong>();
            if (App == 0 && actions.Length == 0)
                return "All Apps & Actions";
            var plural = (actions.Length == 1 ? "" : "s");
            return $"App #{App} {(actions.Length > 0 ? $"Action{plural} {actions.WithCommas(noSpaces: true)}" : "All Actions")}";
        }
    }
}
