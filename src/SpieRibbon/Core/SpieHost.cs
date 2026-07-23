using System;
using Autodesk.Revit.UI;
using SpieRibbon.Contracts;

namespace SpieRibbon.Core
{
    /// <summary>
    /// The host services handed to every module. Thin wrapper over <see cref="RevitContextRunner"/>.
    /// </summary>
    public class SpieHost : ISpieHost
    {
        private readonly RevitContextRunner _runner;

        public SpieHost(RevitContextRunner runner)
        {
            _runner = runner;
        }

        public void RunInRevitContext(Action<UIApplication> action)
        {
            _runner.Enqueue(action);
        }
    }
}
