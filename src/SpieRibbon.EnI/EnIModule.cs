using System.Collections.Generic;
using SpieRibbon.Contracts;

namespace SpieRibbon.EnI
{
    public class EnIModule : ISpieModule
    {
        public string Name => "SPIE E&I";

        // Placeholder - no tools yet. The toolbox shows "No tools available yet." when enabled.
        public List<ToolGroup> BuildGroups(ISpieHost host) => new List<ToolGroup>();
    }
}
