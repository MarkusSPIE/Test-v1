using System;
using System.Collections.Concurrent;
using Autodesk.Revit.UI;

namespace SpieRibbon.Core
{
    /// <summary>
    /// A single, shared ExternalEvent that runs queued actions in a valid Revit API context.
    /// Modules enqueue work (via <see cref="SpieHost"/>) instead of each creating their own
    /// ExternalEvent. Created once at add-in startup.
    /// </summary>
    public class RevitContextRunner : IExternalEventHandler
    {
        private readonly ConcurrentQueue<Action<UIApplication>> _queue =
            new ConcurrentQueue<Action<UIApplication>>();

        private ExternalEvent _externalEvent;

        /// <summary>Must be called from a valid context (add-in OnStartup).</summary>
        public void Initialize()
        {
            _externalEvent = ExternalEvent.Create(this);
        }

        public void Enqueue(Action<UIApplication> action)
        {
            if (action == null)
                return;

            _queue.Enqueue(action);
            _externalEvent.Raise();
        }

        public void Execute(UIApplication app)
        {
            while (_queue.TryDequeue(out Action<UIApplication> action))
            {
                try
                {
                    action(app);
                }
                catch (Exception ex)
                {
                    // One tool failing must not kill the queue or the toolbox.
                    TaskDialog.Show("SPIE Ribbon", "A tool failed:\n\n" + ex.Message);
                }
            }
        }

        public string GetName() => "SPIE Ribbon Context Runner";
    }
}
