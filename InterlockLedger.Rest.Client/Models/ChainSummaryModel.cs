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
using System.Linq;
using System.Text.RegularExpressions;

namespace InterlockLedger.Rest.Client
{
    public sealed class ChainSummaryModel : ChainIdModel
    {
        /// <summary>
        /// List of active apps (only the numeric ids)
        /// </summary>
        public List<ulong> ActiveApps { get; set; }

        /// <summary>
        /// Description (perhaps intended primary usage) [Optional]
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Is this record not able to accept new records?
        /// </summary>
        public bool IsClosedForNewTransactions { get; set; }

        /// <summary>
        /// Last record (serial number)
        /// </summary>
        public ulong LastRecord { get; set; }

        public DateTimeOffset? LastUpdate { get; set; }

        /// <summary>
        /// Chain has a license
        /// </summary>
        public bool Licensed => !LicensingStatus.Safe().StartsWith("Unlicensed:", StringComparison.OrdinalIgnoreCase);

        public ulong[] LicensedApps {
            get {
                var match = Regex.Match(LicensingStatus.Safe(), @"Apps: \[([\d,]+)\]");
                if (match.Success)
                    if (match.Groups.Count > 1) {
                        string[] values = match.Groups[1].Value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                        return values.Select(s => ulong.Parse(s)).ToArray();
                    }

                return Array.Empty<ulong>();
            }
        }

        /// <summary>
        /// If license (or default trial period) is expired
        /// </summary>
        public bool LicenseIsExpired => LicensingStatus.Safe().Contains("Expired since", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        ///  Composite licensing status
        /// </summary>
        public string LicensingStatus { get; set; }

        /// <summary>
        /// Size in bytes the chain occupies in storage
        /// </summary>
        public ulong? SizeInBytes { get; set; }
    }
}