using System.Collections.Generic;
using SpieRibbon.Contracts;

namespace SpieRibbon.Civil
{
    public class CivilModule : ISpieModule
    {
        public string Name => "SPIE Civil";

        // Placeholder - no tools yet. The toolbox shows "No tools available yet." when enabled.
        public List<ToolGroup> BuildGroups(ISpieHost host) => new List<ToolGroup>();
    }
}
