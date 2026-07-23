using System.Collections.Generic;
using SpieRibbon.Contracts;

namespace SpieRibbon.UI
{
    /// <summary>
    /// Sidebar display metadata (short label + icon) per module name. Deliberately host-only,
    /// not part of ISpieModule - keeps the module contract minimal. Unknown module names (a
    /// future module the host doesn't have an entry for) fall back gracefully rather than
    /// breaking, so a new module always shows up even before someone updates this list.
    /// </summary>
    internal static class CategoryDisplay
    {
        private static readonly Dictionary<string, string> ShortLabels = new Dictionary<string, string>
        {
            { "SPIE Algemeen", "Algemeen" },
            { "SPIE BIM Management", "BIM" },
            { "SPIE Civil", "Civil" },
            { "SPIE E&I", "E&I" },
            { "SPIE HVAC", "HVAC" }
        };

        private static readonly Dictionary<string, string> CategoryIcons = new Dictionary<string, string>
        {
            { "SPIE Algemeen", ToolIcons.Manage },
            { "SPIE BIM Management", ToolIcons.Building },
            { "SPIE Civil", ToolIcons.Directions },
            { "SPIE E&I", ToolIcons.LightningBolt },
            { "SPIE HVAC", ToolIcons.Cloud }
        };

        public static string GetShortLabel(string moduleName)
        {
            if (ShortLabels.TryGetValue(moduleName, out string label))
                return label;

            const string prefix = "SPIE ";
            return moduleName.StartsWith(prefix) ? moduleName.Substring(prefix.Length) : moduleName;
        }

        public static string GetIcon(string moduleName)
        {
            return CategoryIcons.TryGetValue(moduleName, out string icon) ? icon : ToolIcons.Manage;
        }
    }
}
