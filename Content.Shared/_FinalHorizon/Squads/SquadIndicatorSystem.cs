using Content.Shared.Database;
using Content.Shared.Examine;
using Content.Shared.IdentityManagement;
using Content.Shared.Mobs.Systems;
using Content.Shared.Verbs;
using Robust.Shared.Utility;

namespace Content.Shared._FinalHorizon.Squads;

public sealed partial class SquadIndicatorSystem : EntitySystem
{
    [Dependency] private readonly MobStateSystem _mobState = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SquadIndicatorComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<SquadIndicatorComponent, GetVerbsEvent<Verb>>(OnGetVerbs);
    }

    private void OnExamined(EntityUid uid, SquadIndicatorComponent comp, ref ExaminedEvent args)
    {
        var desc = comp.CurrentSquad switch
        {
            <= SquadTeams.White => "squad-indicator-team-white",
            <= SquadTeams.Red => "squad-indicator-team-red",
            <= SquadTeams.Blue => "squad-indicator-team-blue",
            <= SquadTeams.Yellow => "squad-indicator-team-yellow",
            <= SquadTeams.Green => "squad-indicator-team-green",
            <= SquadTeams.Orange => "squad-indicator-team-orange",
            <= SquadTeams.Purple => "squad-indicator-team-purple",
            > SquadTeams.Purple => "squad-indicator-team-white",
        };

        args.PushMarkup(Loc.GetString(desc, ("target", Identity.Entity(uid, EntityManager))));
    }

    private void OnGetVerbs(EntityUid uid, SquadIndicatorComponent comp, ref GetVerbsEvent<Verb> args)
    {
        if (args.User != uid || _mobState.IsIncapacitated(uid))
            return;

        Verb verbWhite = new()
        {
            Text = Loc.GetString("squad-indicator-verb-white"),
            Act = () => comp.CurrentSquad = SquadTeams.White,
            Category = VerbCategory.JoinSquads,
        };
        args.Verbs.Add(verbWhite);

        Verb verbRed = new()
        {
            Text = Loc.GetString("squad-indicator-verb-red"),
            Act = () => comp.CurrentSquad = SquadTeams.Red,
            Category = VerbCategory.JoinSquads,
        };
        args.Verbs.Add(verbRed);

        Verb verbBlue = new()
        {
            Text = Loc.GetString("squad-indicator-verb-blue"),
            Act = () => comp.CurrentSquad = SquadTeams.Blue,
            Category = VerbCategory.JoinSquads,
        };
        args.Verbs.Add(verbBlue);

        Verb verbYellow = new()
        {
            Text = Loc.GetString("squad-indicator-verb-yellow"),
            Act = () => comp.CurrentSquad = SquadTeams.Yellow,
            Category = VerbCategory.JoinSquads,
        };
        args.Verbs.Add(verbYellow);

        Verb verbGreen = new()
        {
            Text = Loc.GetString("squad-indicator-verb-green"),
            Act = () => comp.CurrentSquad = SquadTeams.Green,
            Category = VerbCategory.JoinSquads,
        };
        args.Verbs.Add(verbGreen);

        Verb verbOrange = new()
        {
            Text = Loc.GetString("squad-indicator-verb-orange"),
            Act = () => comp.CurrentSquad = SquadTeams.Orange,
            Category = VerbCategory.JoinSquads,
        };
        args.Verbs.Add(verbOrange);

        Verb verbPurple = new()
        {
            Text = Loc.GetString("squad-indicator-verb-purple"),
            Act = () => comp.CurrentSquad = SquadTeams.Purple,
            Category = VerbCategory.JoinSquads,
        };
        args.Verbs.Add(verbPurple);
    }
}
