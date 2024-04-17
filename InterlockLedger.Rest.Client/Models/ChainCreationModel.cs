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
/// Chain creation parameters
/// </summary>
public class ChainCreationModel
{
    /// <summary>
    /// List of additional apps (only the numeric ids)
    /// </summary>
    public List<ulong> AdditionalApps { get; set; } = [];

    /// <summary>
    /// API certificates to authorize with corresponding permissions
    /// </summary>
    public IEnumerable<CertificatePermitModel>? ApiCertificates { get; set; }
    /// <summary>
    /// Description (perhaps intended primary usage) [Optional]
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Emergency closing key password [Required]
    /// </summary>
    public string? EmergencyClosingKeyPassword { get; set; }

    /// <summary>
    /// Emergency closing key strength of key (default: ExtraStrong)
    /// </summary>
    public KeyStrength EmergencyClosingKeyStrength { get; set; } = KeyStrength.ExtraStrong;

    /// <summary>
    /// Keys algorithm (default: EdDSA)
    /// </summary>
    public Algorithms KeysAlgorithm { get; set; } = Algorithms.EdDSA;

    /// <summary>
    /// App/Key management key password [Required]
    /// </summary>
    public string? ManagementKeyPassword { get; set; }

    /// <summary>
    /// App/Key management strength of key (default: Strong)
    /// </summary>
    public KeyStrength ManagementKeyStrength { get; set; } = KeyStrength.Strong;

    /// <summary>
    /// Name [Required]
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Operating key strength of key (default: Normal)
    /// </summary>
    public KeyStrength OperatingKeyStrength { get; set; } = KeyStrength.Normal;

    /// <summary>
    /// Keys algorithm (default: EdDSA)
    /// </summary>
    public Algorithms OperatingKeyAlgorithm { get; set; } = Algorithms.EdDSA;


    /// <summary>
    /// Parent record Id [Optional]
    /// </summary>
    public string? Parent { get; set; }
}
