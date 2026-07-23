using System.Windows.Media;

namespace SpieRibbon.Chrome
{
    /// <summary>
    /// Brand colors, pixel-sampled from the actual SPIE logo (see DESIGN-SYSTEM.md). Single
    /// source of truth - every window should reference these rather than re-declaring hex
    /// literals, so the colors can't drift between files.
    /// </summary>
    public static class SpieColors
    {
        public static readonly SolidColorBrush Navy = new SolidColorBrush(Color.FromRgb(0x00, 0x37, 0x72));
        public static readonly SolidColorBrush Red = new SolidColorBrush(Color.FromRgb(0xE3, 0x1C, 0x18));
    }
}
