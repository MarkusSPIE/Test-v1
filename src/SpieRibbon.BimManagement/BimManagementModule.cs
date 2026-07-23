using System.Collections.Generic;
using SpieRibbon.BimManagement.IfcExport;
using SpieRibbon.Contracts;

namespace SpieRibbon.BimManagement
{
    public class BimManagementModule : ISpieModule
    {
        public string Name => "SPIE BIM Management";

        public List<ToolGroup> BuildGroups(ISpieHost host)
        {
            return new List<ToolGroup>
            {
                new ToolGroup
                {
                    Name = "Import/Export",
                    Tools = new List<ToolItem>
                    {
                        new ToolItem
                        {
                            Label = "IFC Export (2GW)",
                            Tooltip = "Export selected views to IFC, setting Building Name to match each view before export.",
                            Version = "v0.1",
                            Action = () => host.RunInRevitContext(IfcBatchExporter.Run)
                        }
                    }
                }
            };
        }
    }
}
