using System.Collections.Generic;
using SpieRibbon.Contracts;

namespace SpieRibbon.Hvac
{
    public class HvacModule : ISpieModule
    {
        public string Name => "SPIE HVAC";

        // Placeholder - no tools yet. The toolbox shows "No tools available yet." when enabled.
        public List<ToolGroup> BuildGroups(ISpieHost host) => new List<ToolGroup>();
    }
}
