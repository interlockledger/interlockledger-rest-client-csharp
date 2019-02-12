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

using System.Collections.Generic;
using System.Drawing;

namespace InterlockLedger
{
    /// <summary>
    /// Peer details
    /// </summary>
    public sealed class PeerModel
    {
        /// <summary>
        /// Network address to contact the peer
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// Mapping color
        /// </summary>
        public Color Color { get; set; }

        /// <summary>
        /// Unique node id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Node name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Network this node participates on
        /// </summary>
        public string Network { get; set; }

        /// <summary>
        /// Node owner id [Optional]
        /// </summary>
        public string OwnerId { get; set; }

        /// <summary>
        /// Node owner name [Optional]
        /// </summary>
        public string OwnerName { get; set; }

        /// <summary>
        /// Port the peer is listening
        /// </summary>
        public ushort Port { get; set; }

        /// <summary>
        /// Network protocol the peer is listening
        /// </summary>
        public string Protocol { get; set; }

        /// <summary>
        /// List of active roles running in the node
        /// </summary>
        public IEnumerable<string> Roles { get; set; }
    }
}