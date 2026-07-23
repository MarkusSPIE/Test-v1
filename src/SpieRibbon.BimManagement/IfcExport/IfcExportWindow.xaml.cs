using System.Collections.Generic;
using System.Linq;
using System.Windows;
using SpieRibbon.Chrome;

namespace SpieRibbon.BimManagement.IfcExport
{
    public partial class IfcExportWindow : Window
    {
        public List<ViewListItem> SelectedViews { get; private set; } = new List<ViewListItem>();
        public IfcVersionOption SelectedIfcVersion { get; private set; }
        public bool ExportBaseQuantities { get; private set; }

        public IfcExportWindow(List<ViewListItem> views)
        {
            InitializeComponent();
            SpieChrome.Apply(this, TitleBarHost, "IFC export (2GW)");
            ViewListBox.ItemsSource = views;

            IfcVersionCombo.ItemsSource = IfcVersionOption.All;
            IfcVersionCombo.SelectedIndex = 1; // IFC 2x3 Coordination View 2.0 - broadly compatible default
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedViews = ViewListBox.SelectedItems.Cast<ViewListItem>().ToList();
            if (SelectedViews.Count == 0)
            {
                System.Windows.MessageBox.Show(this, "Select at least one view.", "SPIE Ribbon",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SelectedIfcVersion = (IfcVersionOption)IfcVersionCombo.SelectedItem;
            ExportBaseQuantities = ExportBaseQuantitiesCheck.IsChecked == true;

            DialogResult = true;
        }
    }
}
