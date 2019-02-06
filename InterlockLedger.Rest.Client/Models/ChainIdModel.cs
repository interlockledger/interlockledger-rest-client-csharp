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

namespace InterlockLedger
{
    /// <summary>
    /// Chain Id
    /// </summary>
    public sealed class ChainIdModel : IComparable<ChainIdModel>, IEquatable<ChainIdModel>
    {
        /// <summary>
        /// Unique record id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Name [Optional]
        /// </summary>
        public string Name { get; set; }

        /// <summary>Compares the current instance with another ChainIdModel instance and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.</summary>
        /// <param name="other">Another ChainIdModel instance to compare with this instance.</param>
        /// <returns>A value that indicates the relative order of the objects being compared. The return value has these meanings:
        ///   Value meaning :
        ///
        ///   Less than zero
        ///         This instance precedes <paramref name="other">other</paramref> in the sort order.
        ///
        ///   Zero
        ///         This instance occurs in the same position in the sort order as <paramref name="other">other</paramref>.
        ///
        ///   Greater than zero
        ///         This instance follows <paramref name="other">other</paramref> in the sort order.
        ///  </returns>
        public int CompareTo(ChainIdModel other) => Id.CompareTo(other?.Id);

        /// <summary>
        /// Compares this ChainIdModel to other object
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>True if obj is a ChainIdModel and is equal to this</returns>
        public override bool Equals(object obj) => Equals(obj as ChainIdModel);

        /// <summary>
        /// Compares this ChainIdModel to other instance
        /// </summary>
        /// <param name="other"></param>
        /// <returns>True if other is equal to this</returns>
        public bool Equals(ChainIdModel other) => other != null && Id == other.Id;

        /// <summary>Calculate the hash</summary>
        /// <returns>A hash code for the current instance</returns>
        public override int GetHashCode() => Id?.GetHashCode() ?? 0;

        public override string ToString() => $"Chain '{Name}' #{Id}";
    }
}