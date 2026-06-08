using Content.Client.GameTicking.Managers;
using Content.Shared._FinalHorizon.Roles;
using Content.Shared.Roles;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using Robust.Shared.Prototypes;
using System.Numerics;
using static Robust.Client.UserInterface.Controls.BoxContainer;

namespace Content.Client._FinalHorizon.Roles.UI;

public sealed partial class RosterGui : DefaultWindow
{
    private readonly RosterSystem _rosterSystem;
    [Dependency] private readonly IEntitySystemManager _entitySystem = default!;

    private List<Roster> _rosterList = new();

    private readonly Control _base;
    public RosterGui()
    {
        IoCManager.InjectDependencies(this);
        _rosterSystem = _entitySystem.GetEntitySystem<RosterSystem>();

        Title = "Roster Menu";

        _rosterList = _rosterSystem.CachedRosters;

        _base = new BoxContainer()
        {
            Orientation = LayoutOrientation.Vertical,
        };
        ContentsContainer.AddChild(_base);

        BuildUi();
        _rosterSystem.RosterUpdateAction += UpdateRoster;
    }

    private void BuildUi()
    {
        _base.RemoveAllChildren();
        var scrollingContainer = new ScrollContainer()
        {
            HorizontalExpand = true,
            VerticalExpand = true,
        };
        _base.AddChild(scrollingContainer);
        var rosterBoxContainer = new BoxContainer()
        {
            Orientation = LayoutOrientation.Vertical,
            VerticalAlignment = VAlignment.Top,
        };
        scrollingContainer.AddChild(rosterBoxContainer);
        foreach (var roster in _rosterList)
        {
            var rosterBox = new BoxContainer
            {
                Margin = new Thickness(10),
                Orientation = LayoutOrientation.Vertical,
            };
            var rosterTitle = new Label()
            {
                Margin = new Thickness(0, 0, 0, 5),
                HorizontalAlignment = HAlignment.Left,
                Text = roster.Name
            };
            rosterBox.AddChild(rosterTitle);
            foreach (var squad in roster.Squads)
            {
                var squadBox = new BoxContainer
                {
                    Orientation = LayoutOrientation.Vertical,
                };
                var squadTitle = new Label()
                {
                    Margin = new Thickness(0, 0, 0, 5),
                    HorizontalAlignment = HAlignment.Left,
                    Text = squad.Name
                };
                squadBox.AddChild(squadTitle);
                foreach (var slot in squad.Slots)
                {
                    var jobButton = new RosterSlotButton(slot.RosterId, slot.SquadId, slot.SlotId)
                    {
                        Text = slot.Job.ToString(),
                        HorizontalExpand = true,
                        Margin = new Thickness(0, 0, 0, 5)
                    };
                    jobButton.OnPressed += _ => _rosterSystem.RequestSlot(slot.RosterId ?? 0, );
                    squadBox.AddChild(jobButton);
                }


                rosterBox.AddChild(squadBox);
            }

            rosterBoxContainer.AddChild(rosterBox);
        }
    }

    public void UpdateRoster(List<Roster> rosters)
    {
        _rosterList = rosters;
        BuildUi();
    }

    sealed class RosterSlotButton : Button
    {
        public int? RosterId { get; }
        public int? SquadId { get; }

        public int? SlotId { get; }

        public RosterSlotButton(int? rosterId, int? squadId, int? slotId)
        {
            RosterId = rosterId;
            SquadId = squadId;
            SlotId = slotId;
        }
    }
}
