using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Autodesk.Revit.UI;
using SpieRibbon.Core;
using SpieRibbon.UI;
using SpieRibbon.Chrome;

namespace SpieRibbon
{
    public class Application : IExternalApplication
    {
        private const string TabName = "SPIE";
        private const string PanelName = "SPIE Ribbon";

        // Auto-generated (see VersionInfo.g.cs) - don't hand-edit this. To bump the version,
        // edit deploy\VERSION.txt (auto-patch-bumped by Build-Package.ps1 on every package).
        internal const string VersionLabel = VersionInfo.Label;

        // Session-wide state.
        internal static SpieHost Host;
        internal static SettingsStore Settings;
        internal static SpieToolsWindow ToolsWindow;

        private static RevitContextRunner _runner;
        private static string _modulesDir;
        private static List<LoadedModule> _modules;
        private static readonly List<string> _loadWarnings = new List<string>();

        public Result OnStartup(UIControlledApplication application)
        {
            // Forces SpieRibbon.Ui.dll to load now, before any module code runs (modules
            // reference it with Private=false, expecting the host's copy to already be
            // resolvable). Same reasoning as Host/Contracts being wired up here first.
            _ = typeof(SpieChrome);

            _runner = new RevitContextRunner();
            _runner.Initialize();
            Host = new SpieHost(_runner);
            Settings = SettingsStore.Load();

            string year = application.ControlledApplication.VersionNumber;
            _modulesDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "SpieRibbon", "Modules", year);

            try
            {
                application.CreateRibbonTab(TabName);
            }
            catch (Exception)
            {
                // Tab already exists (e.g. add-in reloaded) - safe to ignore.
            }

            RibbonPanel panel = application.CreateRibbonPanel(TabName, PanelName);
            string assemblyPath = Assembly.GetExecutingAssembly().Location;

            var openToolboxButton = new PushButtonData(
                "OpenToolbox",
                "SPIE" + Environment.NewLine + "Ribbon",
                assemblyPath,
                "SpieRibbon.Commands.OpenToolboxCommand")
            {
                ToolTip = "Open SPIE Ribbon - a floating, resizable window with all SPIE tools."
            };
            panel.AddItem(openToolboxButton);

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application) => Result.Succeeded;

        /// <summary>
        /// Discovers and loads modules once per session (all discovered modules load regardless
        /// of enabled state; 'enabled' only controls visibility). Newly discovered modules are
        /// registered as disabled. Newly *deployed* modules appear after a Revit restart.
        /// </summary>
        internal static void EnsureModulesLoaded()
        {
            if (_modules != null)
                return;

            var loader = new ModuleLoader(_modulesDir);
            _modules = loader.LoadAll(Host);
            _loadWarnings.Clear();
            _loadWarnings.AddRange(loader.Warnings);

            Settings.EnsureKnown(_modules.Select(m => m.Name));
            Settings.Save();
        }

        internal static IReadOnlyList<LoadedModule> AllModules => _modules ?? new List<LoadedModule>();

        internal static IReadOnlyList<string> LoadWarnings => _loadWarnings;

        internal static IEnumerable<LoadedModule> EnabledModules()
        {
            return AllModules.Where(m => Settings.IsEnabled(m.Name));
        }
    }
}
