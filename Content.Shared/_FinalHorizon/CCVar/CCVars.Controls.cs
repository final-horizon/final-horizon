using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

public sealed partial class CCVars
{
    /// <summary>
    /// Will early out of smart equips when false.
    /// </summary>
    public static readonly CVarDef<bool> AllowSmartEquip =
        CVarDef.Create("finalhorizon.allowsmartequip", false, CVar.SERVER | CVar.REPLICATED);
}
