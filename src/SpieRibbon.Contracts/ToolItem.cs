using System;

namespace SpieRibbon.Contracts
{
    /// <summary>
    /// A single clickable tool in the toolbox. Modules provide a plain <see cref="Action"/> -
    /// no WPF or ICommand knowledge required. Actions that touch the Revit API should route
    /// through <see cref="ISpieHost.RunInRevitContext"/> rather than calling Revit directly.
    /// </summary>
    public class ToolItem
    {
        public string Label { get; set; }
        public string Tooltip { get; set; }
        public Action Action { get; set; }

        /// <summary>Per-tool version, e.g. "v0.1". Shown appended to the tooltip.</summary>
        public string Version { get; set; } = "v0.1";

        /// <summary>Segoe MDL2 Assets glyph (see <see cref="ToolIcons"/>), or null for no icon.</summary>
        public string IconGlyph { get; set; }
    }
}
