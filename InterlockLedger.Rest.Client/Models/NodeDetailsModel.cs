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

/// <summary>
/// Node details
/// </summary>
public class NodeDetailsModel : NodeCommonModel
{
    /// <summary>
    /// List of owned chains, only the ids
    /// </summary>
    public IEnumerable<string> Chains { get; set; } = [];

    /// <summary>
    /// Other properties the node may have
    /// </summary>
    public Dictionary<string, string> Extensions { get; set; } = [];

    private static string ToLine(KeyValuePair<string, string> x) => $"{x.Key}: {x.Value}";

    private static readonly string _itemSeparator = $"{Environment.NewLine} - ";

    private static string ToList(string label, IEnumerable<string> items) => items.PrependedBy(label).JoinedBy(_itemSeparator);

    protected override string Extras => $"""
        {ToList("Extensions:", Extensions.Select(ToLine))}
        {ToList("Chains:", Chains)}
        """;
}