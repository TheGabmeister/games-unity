namespace SMW
{
    public enum SlopeKind
    {
        SteepL,
        SteepR,
        ShallowL,
        ShallowR,
    }

    public static class SlopeKindExtensions
    {
        // Canonical asset name — matches the SVG and prefab file names on disk
        // (`Slope_Steep_L`, `Slope_Shallow_R`, …). Enum.ToString() drops the
        // underscore, which doesn't match the files, so asset lookups must route
        // through this helper.
        public static string AssetName(this SlopeKind kind) => kind switch
        {
            SlopeKind.SteepL => "Slope_Steep_L",
            SlopeKind.SteepR => "Slope_Steep_R",
            SlopeKind.ShallowL => "Slope_Shallow_L",
            SlopeKind.ShallowR => "Slope_Shallow_R",
            _ => "Slope_Unknown",
        };
    }
}
