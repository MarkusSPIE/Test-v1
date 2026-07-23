using System;
using Autodesk.Revit.UI;

namespace SpieRibbon.Contracts
{
    /// <summary>
    /// Services the host provides to modules. Passed to <see cref="ISpieModule.BuildGroups"/>.
    /// </summary>
    public interface ISpieHost
    {
        /// <summary>
        /// Runs <paramref name="action"/> in a valid Revit API context (via a shared
        /// ExternalEvent). A modeless window's button click is NOT a valid context on its own,
        /// so any Revit API work triggered from the toolbox must go through here. Returns
        /// immediately; the action runs when Revit next processes the event.
        /// </summary>
        void RunInRevitContext(Action<UIApplication> action);
    }
}
