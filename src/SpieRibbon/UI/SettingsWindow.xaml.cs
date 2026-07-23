using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SpieRibbon.Core;

namespace SpieRibbon.UI
{
    public partial class SettingsWindow : Window
    {
        private readonly List<CheckBox> _checkBoxes = new List<CheckBox>();

        public SettingsWindow()
        {
            InitializeComponent();
            BuildList();
        }

        private void BuildList()
        {
            if (Application.AllModules.Count == 0)
            {
                ModuleCheckList.Children.Add(new TextBlock
                {
                    Text = "No modules installed.",
                    Foreground = Brushes.Gray,
                    TextWrapping = TextWrapping.Wrap
                });
                OkButton.IsEnabled = false;
                return;
            }

            foreach (LoadedModule module in Application.AllModules)
            {
                var checkBox = new CheckBox
                {
                    Content = module.Name,
                    IsChecked = Application.Settings.IsEnabled(module.Name),
                    Margin = new Thickness(0, 4, 0, 4),
                    Tag = module.Name
                };
                _checkBoxes.Add(checkBox);
                ModuleCheckList.Children.Add(checkBox);
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (CheckBox checkBox in _checkBoxes)
                Application.Settings.Set((string)checkBox.Tag, checkBox.IsChecked == true);

            DialogResult = true;
        }
    }
}
