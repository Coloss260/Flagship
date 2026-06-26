namespace Content.FlagShip.Server.Planets;

[RegisterComponent]
public sealed partial class PlanetMapComponent : Component
{
    [DataField]
    public string Parallax = "bedrock";
}
// Only excludes a grid from garbage clean really.
