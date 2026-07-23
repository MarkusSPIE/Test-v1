using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Interop;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ClosedXML.Excel;
using Microsoft.Win32;

namespace SpieRibbon.Algemeen.ExportSchedule
{
    /// <summary>
    /// Orchestrates the Export Schedule tool: pick schedules, choose a file, write .xlsx.
    /// Runs inside a valid Revit API context (invoked via ISpieHost.RunInRevitContext).
    /// </summary>
    public static class ScheduleExporter
    {
        public static void Run(UIApplication app)
        {
            Document doc = app.ActiveUIDocument.Document;

            List<ScheduleListItem> schedules = new FilteredElementCollector(doc)
                .OfClass(typeof(ViewSchedule))
                .Cast<ViewSchedule>()
                .Where(vs => !vs.IsTemplate && !vs.Name.StartsWith("<"))
                .OrderBy(vs => vs.Name)
                .Select(vs => new ScheduleListItem { Name = vs.Name, Id = vs.Id })
                .ToList();

            if (schedules.Count == 0)
            {
                TaskDialog.Show("SPIE Ribbon", "No schedules were found in this project.");
                return;
            }

            var picker = new ScheduleExportWindow(schedules);
            new WindowInteropHelper(picker).Owner = app.MainWindowHandle;

            if (picker.ShowDialog() != true || picker.SelectedSchedules.Count == 0)
                return;

            string defaultFileName = picker.SelectedSchedules.Count == 1
                ? SanitizeFileName(picker.SelectedSchedules[0].Name)
                : "Schedules";

            var saveDialog = new SaveFileDialog
            {
                Filter = "Excel Workbook (*.xlsx)|*.xlsx",
                FileName = defaultFileName,
                DefaultExt = ".xlsx"
            };

            if (saveDialog.ShowDialog() != true)
                return;

            try
            {
                ExportToWorkbook(doc, picker.SelectedSchedules, saveDialog.FileName);
                TaskDialog.Show("SPIE Ribbon",
                    string.Format("Exported {0} schedule(s) to:\n{1}", picker.SelectedSchedules.Count, saveDialog.FileName));
            }
            catch (Exception ex)
            {
                TaskDialog.Show("SPIE Ribbon - Export Failed", ex.Message);
            }
        }

        private static void ExportToWorkbook(Document doc, List<ScheduleListItem> selected, string targetPath)
        {
            string tempFolder = Path.Combine(Path.GetTempPath(), "SpieRibbon_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempFolder);

            try
            {
                using (var workbook = new XLWorkbook())
                {
                    var usedSheetNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                    foreach (ScheduleListItem item in selected)
                    {
                        var schedule = doc.GetElement(item.Id) as ViewSchedule;
                        if (schedule == null)
                            continue;

                        string tempFileName = Guid.NewGuid().ToString("N") + ".txt";
                        var options = new ViewScheduleExportOptions
                        {
                            ColumnHeaders = ExportColumnHeaders.OneRow,
                            Title = false,
                            HeadersFootersBlanks = false,
                            TextQualifier = ExportTextQualifier.None,
                            FieldDelimiter = "\t"
                        };

                        schedule.Export(tempFolder, tempFileName, options);

                        string exportedFile = Path.Combine(tempFolder, tempFileName);
                        string[] lines = File.Exists(exportedFile) ? File.ReadAllLines(exportedFile) : Array.Empty<string>();

                        string sheetName = MakeUniqueSheetName(item.Name, usedSheetNames);
                        var worksheet = workbook.Worksheets.Add(sheetName);

                        for (int row = 0; row < lines.Length; row++)
                        {
                            string[] cells = lines[row].Split('\t');
                            for (int col = 0; col < cells.Length; col++)
                                worksheet.Cell(row + 1, col + 1).Value = cells[col];
                        }

                        worksheet.Columns().AdjustToContents();
                    }

                    workbook.SaveAs(targetPath);
                }
            }
            finally
            {
                try { Directory.Delete(tempFolder, true); } catch { /* best effort cleanup */ }
            }
        }

        private static string MakeUniqueSheetName(string rawName, HashSet<string> usedNames)
        {
            string sanitized = SanitizeSheetName(rawName);
            string candidate = sanitized;
            int suffix = 1;
            while (!usedNames.Add(candidate))
            {
                string suffixText = "_" + suffix++;
                candidate = sanitized.Substring(0, Math.Min(sanitized.Length, 31 - suffixText.Length)) + suffixText;
            }
            return candidate;
        }

        private static string SanitizeSheetName(string name)
        {
            char[] invalid = { '\\', '/', '*', '?', ':', '[', ']' };
            string cleaned = string.Join("_", name.Split(invalid));
            return cleaned.Length > 31 ? cleaned.Substring(0, 31) : cleaned;
        }

        private static string SanitizeFileName(string name)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');
            return name;
        }
    }
}
