using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace SpieRibbon.Core
{
    /// <summary>
    /// Per-user module enable/disable state, stored as a small JSON file under
    /// %AppData%\SpieRibbon\settings.json. Lives outside the per-version DLL folders so it
    /// survives redeploys. New modules default to DISABLED until the user opts in.
    ///
    /// Deliberately dependency-free (no JSON library): the payload is a flat map of
    /// module-name -> bool, parsed with a tolerant regex so hand-edits or reformatting don't
    /// break it.
    /// </summary>
    public class SettingsStore
    {
        private static readonly string SettingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SpieRibbon", "settings.json");

        private readonly Dictionary<string, bool> _enabled =
            new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

        public static SettingsStore Load()
        {
            var store = new SettingsStore();
            try
            {
                if (File.Exists(SettingsPath))
                {
                    string json = File.ReadAllText(SettingsPath);
                    var matches = Regex.Matches(json, "\"((?:[^\"\\\\]|\\\\.)*)\"\\s*:\\s*(true|false)");
                    foreach (Match m in matches)
                    {
                        string name = Unescape(m.Groups[1].Value);
                        bool value = m.Groups[2].Value == "true";
                        store._enabled[name] = value;
                    }
                }
            }
            catch
            {
                // Corrupt/unreadable settings must never block the toolbox - start from defaults.
            }
            return store;
        }

        /// <summary>New (unknown) modules default to disabled.</summary>
        public bool IsEnabled(string moduleName)
        {
            return _enabled.TryGetValue(moduleName, out bool value) && value;
        }

        public bool IsKnown(string moduleName) => _enabled.ContainsKey(moduleName);

        public void Set(string moduleName, bool enabled)
        {
            _enabled[moduleName] = enabled;
        }

        /// <summary>Registers any newly discovered module as disabled without overwriting a known choice.</summary>
        public void EnsureKnown(IEnumerable<string> moduleNames)
        {
            foreach (string name in moduleNames)
            {
                if (!_enabled.ContainsKey(name))
                    _enabled[name] = false;
            }
        }

        public void Save()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath));

                var sb = new StringBuilder();
                sb.AppendLine("{");
                int i = 0;
                foreach (var kvp in _enabled)
                {
                    string comma = (++i < _enabled.Count) ? "," : "";
                    sb.AppendLine(string.Format("  \"{0}\": {1}{2}",
                        Escape(kvp.Key), kvp.Value ? "true" : "false", comma));
                }
                sb.AppendLine("}");

                File.WriteAllText(SettingsPath, sb.ToString());
            }
            catch
            {
                // Best effort - failing to persist a preference shouldn't crash Revit.
            }
        }

        private static string Escape(string s) => s.Replace("\\", "\\\\").Replace("\"", "\\\"");

        private static string Unescape(string s) => s.Replace("\\\"", "\"").Replace("\\\\", "\\");
    }
}
