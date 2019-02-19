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
using System.Text.RegularExpressions;

namespace InterlockLedger.Rest.Client
{
    public class AppsModel
    {
        /// <summary>
        /// Network name
        /// </summary>
        public string Network { get; set; }

        /// <summary>
        /// Currently valid apps for this network
        /// </summary>
        public IEnumerable<PublishedApp> ValidApps { get; set; }

        public class PublishedApp : IComparable<PublishedApp>
        {
            public ulong? AlternativeId { get; set; }
            public Version AppVersion { get; set; }
            public string CompositeName => Safe($"{PublisherName}.{Name}#{AppVersion}");
            public string Description { get; set; }
            public ulong Id { get; set; }

            // TODO: map public IEnumerable<DataModel> DataModels { get; set; }
            public string Name { get; set; }

            public string PublisherId { get; set; }
            public string PublisherName { get; set; }
            public IEnumerable<Range> ReservedILTagIds { get; set; }
            public DateTimeOffset Start { get; set; }
            public ushort Version { get; set; }

            public override string ToString() => $"  #{Id} {CompositeName}   {Environment.NewLine}    {Description}";

            int IComparable<PublishedApp>.CompareTo(PublishedApp other) {
                if (other == null) return 1;
                var idCompare = Id.CompareTo(other.Id);
                return idCompare == 0 ? AppVersion.CompareTo(other.AppVersion) : idCompare;
            }

            private static string Safe(string name) => Regex.Replace(name, @"[\s\\/:""<>|\*\?]+", "_");
        }
    }
}