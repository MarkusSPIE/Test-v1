using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SpieRibbon.Core;
using SpieRibbon.Chrome;

namespace SpieRibbon.UI
{
    public partial class SettingsWindow : Window
    {
        private static readonly FontFamily IconFont = new FontFamily("Segoe MDL2 Assets");
        private static readonly SolidColorBrush NavyBrush = SpieColors.Navy;

        private readonly List<CheckBox> _checkBoxes = new List<CheckBox>();

        public SettingsWindow()
        {
            InitializeComponent();
            SpieChrome.Apply(this, TitleBarHost, "Settings");
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
                var content = new StackPanel { Orientation = Orientation.Horizontal };
                content.Children.Add(new TextBlock
                {
                    Text = CategoryDisplay.GetIcon(module.Name),
                    FontFamily = IconFont,
                    FontSize = 14,
                    Foreground = NavyBrush,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 8, 0)
                });
                content.Children.Add(new TextBlock
                {
                    Text = module.Name,
                    VerticalAlignment = VerticalAlignment.Center
                });

                var checkBox = new CheckBox
                {
                    Content = content,
                    IsChecked = Application.Settings.IsEnabled(module.Name),
                    Margin = new Thickness(0, 6, 0, 6),
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
