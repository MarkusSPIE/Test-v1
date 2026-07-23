using System.Windows;
using System.Windows.Interop;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace SpieRibbon.Commands
{
    [Transaction(TransactionMode.ReadOnly)]
    public class OpenToolboxCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Application.EnsureModulesLoaded();

            if (Application.ToolsWindow == null || !Application.ToolsWindow.IsLoaded)
            {
                Application.ToolsWindow = new UI.SpieToolsWindow();
                new WindowInteropHelper(Application.ToolsWindow).Owner = commandData.Application.MainWindowHandle;
                Application.ToolsWindow.RefreshModules();
                Application.ToolsWindow.Show();
            }
            else
            {
                if (Application.ToolsWindow.WindowState == WindowState.Minimized)
                    Application.ToolsWindow.WindowState = WindowState.Normal;
                Application.ToolsWindow.Activate();
            }

            return Result.Succeeded;
        }
    }
}
