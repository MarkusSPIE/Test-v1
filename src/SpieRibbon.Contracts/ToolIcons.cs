namespace SpieRibbon.Contracts
{
    /// <summary>
    /// Segoe MDL2 Assets glyph codepoints for tool/category icons. Segoe MDL2 Assets (not the
    /// newer Segoe Fluent Icons) is used because it ships on every Windows 10/11 machine -
    /// Fluent Icons needs a Windows build a colleague's PC might not have. The two fonts share
    /// codepoints for these common glyphs, so this list stays valid if Fluent Icons is ever
    /// adopted later.
    /// </summary>
    public static class ToolIcons
    {
        public static readonly string Settings = char.ConvertFromUtf32(0xE713);
        public static readonly string ChevronDown = char.ConvertFromUtf32(0xE70D);
        public static readonly string Folder = char.ConvertFromUtf32(0xE8B7);
        public static readonly string Book = char.ConvertFromUtf32(0xE8F1);
        public static readonly string Upload = char.ConvertFromUtf32(0xE898);
        public static readonly string Building = char.ConvertFromUtf32(0xEC06);
        public static readonly string Manage = char.ConvertFromUtf32(0xE912);
        public static readonly string Directions = char.ConvertFromUtf32(0xE8F0);
        public static readonly string LightningBolt = char.ConvertFromUtf32(0xE945);
        public static readonly string Cloud = char.ConvertFromUtf32(0xE753);
    }
}
