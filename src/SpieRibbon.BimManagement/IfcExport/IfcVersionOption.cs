using Autodesk.Revit.DB;

namespace SpieRibbon.BimManagement.IfcExport
{
    public class IfcVersionOption
    {
        public string Label { get; set; }
        public IFCVersion Version { get; set; }

        public override string ToString() => Label;

        public static readonly IfcVersionOption[] All =
        {
            new IfcVersionOption { Label = "IFC 2x3", Version = IFCVersion.IFC2x3 },
            new IfcVersionOption { Label = "IFC 2x3 Coordination View 2.0", Version = IFCVersion.IFC2x3CV2 },
            new IfcVersionOption { Label = "IFC 4", Version = IFCVersion.IFC4 }
        };
    }
}
