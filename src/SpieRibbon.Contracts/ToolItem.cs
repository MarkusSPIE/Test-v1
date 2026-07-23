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
    }
}
