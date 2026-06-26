namespace Content.FlagShip.Server.Worldgen.Components;

/// <summary>
/// This is used to let any entity load chunks and overrides <see cref="LoadingDistance"/> of <see cref="LocalityLoaderComponent"/>
/// </summary>
[RegisterComponent]
public sealed partial class ChunkLoaderComponent : Component
{
    [DataField]
    public int LoadingDistance = 32;

    [DataField]
    public bool RequirePower = false;
}
