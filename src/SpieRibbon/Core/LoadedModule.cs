using System.Collections.Generic;
using SpieRibbon.Contracts;

namespace SpieRibbon.Core
{
    /// <summary>A module that was discovered, instantiated, and had its groups built.</summary>
    public class LoadedModule
    {
        public string Name { get; set; }
        public List<ToolGroup> Groups { get; set; } = new List<ToolGroup>();
    }
}
