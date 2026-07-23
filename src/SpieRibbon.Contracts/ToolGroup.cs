using System.Collections.Generic;

namespace SpieRibbon.Contracts
{
    /// <summary>
    /// A named group of tools shown as a collapsible section inside a module's category.
    /// </summary>
    public class ToolGroup
    {
        public string Name { get; set; }
        public List<ToolItem> Tools { get; set; } = new List<ToolItem>();
    }
}
