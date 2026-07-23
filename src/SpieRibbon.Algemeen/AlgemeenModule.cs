using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using SpieRibbon.Algemeen.ExportSchedule;
using SpieRibbon.Contracts;

namespace SpieRibbon.Algemeen
{
    public class AlgemeenModule : ISpieModule
    {
        public string Name => "SPIE Algemeen";

        public List<ToolGroup> BuildGroups(ISpieHost host)
        {
            return new List<ToolGroup>
            {
                new ToolGroup
                {
                    Name = "Get Started",
                    Tools = new List<ToolItem>
                    {
                        new ToolItem
                        {
                            Label = "Load RFA's",
                            Tooltip = "Open the shared RFA library on the server.",
                            Version = "v0.1",
                            IconGlyph = ToolIcons.Folder,
                            Action = () => OpenFolder(
                                @"G:\SPIE_CET\6_All_disc\02_CAD\07_Revit\SPIE Engineering Hoek\SPIE_NLRS.v3.01\20 Libraries")
                        },
                        new ToolItem
                        {
                            Label = "SPIE Handleidingen",
                            Tooltip = "Open the SPIE manuals folder on the server.",
                            Version = "v0.1",
                            IconGlyph = ToolIcons.Book,
                            Action = () => OpenFolder(
                                @"G:\SPIE_CET\6_All_disc\02_CAD\07_Revit\SPIE Engineering Hoek\SPIE_Handleidingen")
                        }
                    }
                },
                new ToolGroup
                {
                    Name = "Import/Export",
                    Tools = new List<ToolItem>
                    {
                        new ToolItem
                        {
                            Label = "Export Schedule",
                            Tooltip = "Export one or more schedules from this project to Excel.",
                            Version = "v0.1",
                            IconGlyph = ToolIcons.Upload,
                            Action = () => host.RunInRevitContext(ScheduleExporter.Run)
                        }
                    }
                }
            };
        }

        /// <summary>Pure OS action - no Revit API, runs directly on the toolbox UI thread.</summary>
        private static void OpenFolder(string path)
        {
            if (!Directory.Exists(path))
            {
                MessageBox.Show(
                    "Folder not found:" + Environment.NewLine + path,
                    "SPIE Ribbon", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Process.Start("explorer.exe", "\"" + path + "\"");
        }
    }
}
