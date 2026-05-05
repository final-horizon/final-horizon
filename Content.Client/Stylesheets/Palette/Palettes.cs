namespace Content.Client.Stylesheets.Palette;

/// <summary>
///     Stores all style palettes in one accessible location
/// </summary>
/// <remarks>
///     Technically not limited to only colors, can store like, standard padding amounts, and font sizes, maybe?
/// </remarks>
public static class Palettes
{
    // muted tones
    public static readonly ColorPalette Navy = ColorPalette.FromHexBase("#364927", lightnessShift: 0.05f, chromaShift: 0.0045f); // FH start
    public static readonly ColorPalette Cyan = ColorPalette.FromHexBase("#283022", lightnessShift: 0.05f, chromaShift: 0.0045f);
    public static readonly ColorPalette Slate = ColorPalette.FromHexBase("#1f231c");
    public static readonly ColorPalette Neutral = ColorPalette.FromHexBase("#3a3d38"); // FH end

    // status tones
    public static readonly ColorPalette Red = ColorPalette.FromHexBase("#b62124", chromaShift: 0.02f);
    public static readonly ColorPalette Amber = ColorPalette.FromHexBase("#c18e36");
    public static readonly ColorPalette Green = ColorPalette.FromHexBase("#3c854a");
    public static readonly StatusPalette Status = new([Red.Base, Amber.Base, Green.Base]);

    // highlight tones
    public static readonly ColorPalette Gold = ColorPalette.FromHexBase("#8e3434"); // FH
    public static readonly ColorPalette Maroon = ColorPalette.FromHexBase("#9b2236");

    // Intended to be used with `ModulateSelf` to darken / lighten something
    public static readonly ColorPalette AlphaModulate = ColorPalette.FromHexBase("#ffffff");

}
