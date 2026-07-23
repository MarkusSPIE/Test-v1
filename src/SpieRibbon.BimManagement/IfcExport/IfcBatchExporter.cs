using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Interop;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace SpieRibbon.BimManagement.IfcExport
{
    /// <summary>
    /// Orchestrates the IFC Export (2GW) tool: pick IFC settings + views, then for each
    /// selected view (in name order) set Building Name to the view's name and export it.
    /// Building Name is intentionally left at the last exported view's value when done - no
    /// restore step. Runs inside a valid Revit API context (invoked via
    /// ISpieHost.RunInRevitContext).
    /// </summary>
    public static class IfcBatchExporter
    {
        public static void Run(UIApplication app)
        {
            Document doc = app.ActiveUIDocument.Document;

            List<ViewListItem> views = new FilteredElementCollector(doc)
                .OfClass(typeof(View3D))
                .Cast<View3D>()
                .Where(v => !v.IsTemplate)
                .OrderBy(v => v.Name)
                .Select(v => new ViewListItem { Name = v.Name, Id = v.Id })
                .ToList();

            if (views.Count == 0)
            {
                Autodesk.Revit.UI.TaskDialog.Show("SPIE Ribbon", "No 3D views were found in this project.");
                return;
            }

            var picker = new IfcExportWindow(views);
            new WindowInteropHelper(picker).Owner = app.MainWindowHandle;

            if (picker.ShowDialog() != true)
                return;

            var folderDialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Choose a folder for the exported IFC files."
            };

            if (folderDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;

            string folder = folderDialog.SelectedPath;
            List<ViewListItem> orderedViews = picker.SelectedViews.OrderBy(v => v.Name).ToList();

            Parameter buildingNameParam = doc.ProjectInformation.get_Parameter(BuiltInParameter.PROJECT_BUILDING_NAME)
                                           ?? doc.ProjectInformation.LookupParameter("Building Name");

            if (buildingNameParam == null || buildingNameParam.IsReadOnly)
            {
                Autodesk.Revit.UI.TaskDialog.Show("SPIE Ribbon",
                    "Could not find a writable 'Building Name' field in Project Information.");
                return;
            }

            int exported = 0;
            var failures = new StringBuilder();

            foreach (ViewListItem item in orderedViews)
            {
                var view = doc.GetElement(item.Id) as View3D;
                if (view == null)
                    continue;

                try
                {
                    using (var t = new Transaction(doc, "Set Building Name for IFC export"))
                    {
                        t.Start();
                        buildingNameParam.Set(item.Name);
                        t.Commit();
                    }

                    var options = new IFCExportOptions
                    {
                        FileVersion = picker.SelectedIfcVersion.Version,
                        ExportBaseQuantities = picker.ExportBaseQuantities,
                        FilterViewId = view.Id
                    };

                    doc.Export(folder, item.Name + ".ifc", options);
                    exported++;
                }
                catch (Exception ex)
                {
                    failures.AppendLine(string.Format("{0}: {1}", item.Name, ex.Message));
                }
            }

            string summary = string.Format("Exported {0} of {1} view(s) to:\n{2}",
                exported, orderedViews.Count, folder);

            if (failures.Length > 0)
                summary += "\n\nFailed:\n" + failures;

            Autodesk.Revit.UI.TaskDialog.Show("SPIE Ribbon", summary);
        }
    }
}
