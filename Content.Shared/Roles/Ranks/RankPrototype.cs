using Robust.Shared.Prototypes;

namespace Content.Shared.Roles.Ranks;

/// <summary>
///     Used for cosmetic ranks.
/// </summary>
[Prototype]
public sealed partial class RankPrototype : IPrototype
{
    [ViewVariables]
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    ///     The name of the rank.
    /// </summary>
    [DataField(required: true)]
    public string Name { get; set; } = default!;

    /// <summary>
    ///     The shortened version of the rank.
    /// </summary>
    [DataField(required: true)]
    public string Prefix { get; set; } = default!;

    [DataField]
    public string? MalePrefix { get; set; }

    [DataField]
    public string? FemalePrefix { get; set; }

    [DataField]
    public string? Paygrade { get; set; }
}
