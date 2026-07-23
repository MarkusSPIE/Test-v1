using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace SpieRibbon.Algemeen.ExportSchedule
{
    public partial class ScheduleExportWindow : Window
    {
        public List<ScheduleListItem> SelectedSchedules { get; private set; } = new List<ScheduleListItem>();

        public ScheduleExportWindow(List<ScheduleListItem> schedules)
        {
            InitializeComponent();
            ScheduleListBox.ItemsSource = schedules;
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedSchedules = ScheduleListBox.SelectedItems.Cast<ScheduleListItem>().ToList();
            if (SelectedSchedules.Count == 0)
            {
                MessageBox.Show(this, "Select at least one schedule.", "SPIE Ribbon",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DialogResult = true;
        }
    }
}
