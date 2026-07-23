using System.Collections.Generic;

namespace SpieRibbon.Contracts
{
    /// <summary>
    /// One discipline module (e.g. SPIE Algemeen, SPIE Civil). The host discovers every type
    /// implementing this interface in the module folder, instantiates it, and renders its
    /// groups as a category in the toolbox.
    ///
    /// Keep this interface small and stable: changing it forces every module to be recompiled
    /// and redeployed, not just the host. Add new capabilities as new optional interfaces.
    /// </summary>
    public interface ISpieModule
    {
        /// <summary>Category name shown in the toolbox and the settings enable/disable list.</summary>
        string Name { get; }

        /// <summary>
        /// The groups/tools this module contributes. Called once when the module is loaded.
        /// Return an empty list for a module with no tools yet.
        /// </summary>
        List<ToolGroup> BuildGroups(ISpieHost host);
    }
}
