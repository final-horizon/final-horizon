using Robust.Shared.GameStates;

namespace Content.Shared._FinalHorizon.Squads;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SquadIndicatorComponent : Component
{
    [DataField, AutoNetworkedField]
    public SquadTeams CurrentSquad = SquadTeams.White;
}

public enum SquadTeams
{
    White,
    Red,
    Blue,
    Yellow,
    Green,
    Orange,
    Purple,
}
