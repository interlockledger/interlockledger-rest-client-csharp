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
using Newtonsoft.Json;

namespace InterlockLedger.Rest.Client.Abstractions
{
    public static class PageOfExtensions
    {
        public static bool Any<T>(this PageOf<T> page) => page.Safe().Items.SafeAny();

        public static bool Any<T>(this PageOf<T> page, Func<T, bool> predicate) => page.Safe().Items.SafeAny(predicate);

        public static T First<T>(this PageOf<T> page) where T : class => page?.Items?.FirstOrDefault();

        public static T Last<T>(this PageOf<T> page) where T : class => page?.Items?.LastOrDefault();

        public static bool None<T>(this PageOf<T> page) => !page.Any();

        public static bool None<T>(this PageOf<T> page, Func<T, bool> predicate) => !page.Any(predicate);

        public static PageOf<T> Paginate<T>(this IEnumerable<T> resultList, ushort page, byte pageSize) {
            var result = resultList.ToArray();
            if (pageSize == 0)
                return new PageOf<T>(result);
            ushort totalPages = (ushort)Math.Min((result.Length + pageSize - 1) / pageSize, ushort.MaxValue);
            if (page >= totalPages)
                page = totalPages > 0 ? (ushort)(totalPages - 1) : 0;
            return new PageOf<T>(result.Skip(page * pageSize).Take(pageSize).ToArray(), page, pageSize, totalPages);
        }

        public static PageOf<T> Safe<T>(this PageOf<T> page) => page ?? PageOf<T>.Empty;
    }

    public class PageOf<T>
    {
        [JsonConstructor]
        public PageOf(IEnumerable<T> items, ushort page, byte pageSize, ushort totalNumberOfPages) {
            Items = items;
            Page = page;
            PageSize = pageSize;
            TotalNumberOfPages = totalNumberOfPages;
        }

        public PageOf(IEnumerable<T> items) : this(items, 0, 0, items.SafeAny() ? 1 : 0) {
        }

        public static PageOf<T> Empty { get; } = new PageOf<T>(Enumerable.Empty<T>(), 0, 0, 0);
        public IEnumerable<T> Items { get; }
        public ushort Page { get; }
        public byte PageSize { get; }
        public ushort TotalNumberOfPages { get; }

        public PageOf<TN> Cast<TN>() => new PageOf<TN>(Items.Cast<TN>(), Page, PageSize, TotalNumberOfPages);

        public PageOf<TN> Convert<TN>(Func<T, TN> converter) => new PageOf<TN>(Items.Select(converter), Page, PageSize, TotalNumberOfPages);
    }
}