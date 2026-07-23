using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using SpieRibbon.Contracts;
using SpieRibbon.Core;

namespace SpieRibbon.UI
{
    public partial class SpieToolsWindow : Window
    {
        private static readonly FontFamily IconFont = new FontFamily("Segoe MDL2 Assets");
        private static readonly SolidColorBrush NavyBrush = new SolidColorBrush(Color.FromRgb(0x00, 0x37, 0x72));
        private static readonly SolidColorBrush RedBrush = new SolidColorBrush(Color.FromRgb(0xE3, 0x1C, 0x18));
        private static readonly SolidColorBrush SidebarInactiveBrush = new SolidColorBrush(Color.FromRgb(0x5A, 0x5A, 0x5A));

        private LoadedModule _selectedModule;

        public SpieToolsWindow()
        {
            InitializeComponent();
            FooterText.Text = "Created by MVS - " + Application.VersionLabel;
            SettingsGlyph.Text = ToolIcons.Settings;
        }

        /// <summary>Rebuilds the visible tree from the currently enabled modules.</summary>
        public void RefreshModules()
        {
            List<LoadedModule> enabled = Application.EnabledModules().ToList();

            if (_selectedModule == null || enabled.All(m => m.Name != _selectedModule.Name))
                _selectedModule = enabled.FirstOrDefault();

            BuildSidebar(enabled);
            BuildContent();
        }

        private void BuildSidebar(List<LoadedModule> modules)
        {
            SidebarPanel.Children.Clear();

            foreach (LoadedModule module in modules)
            {
                bool isSelected = ReferenceEquals(module, _selectedModule);

                var stack = new StackPanel { Orientation = Orientation.Horizontal };
                stack.Children.Add(new TextBlock
                {
                    Text = CategoryDisplay.GetIcon(module.Name),
                    FontFamily = IconFont,
                    FontSize = 14,
                    Foreground = isSelected ? NavyBrush : SidebarInactiveBrush,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 6, 0)
                });
                stack.Children.Add(new TextBlock
                {
                    Text = CategoryDisplay.GetShortLabel(module.Name),
                    FontSize = 12,
                    FontWeight = isSelected ? FontWeights.Medium : FontWeights.Normal,
                    Foreground = isSelected ? NavyBrush : SidebarInactiveBrush,
                    VerticalAlignment = VerticalAlignment.Center,
                    TextWrapping = TextWrapping.Wrap
                });

                var row = new Border
                {
                    BorderThickness = new Thickness(3, 0, 0, 0),
                    BorderBrush = isSelected ? RedBrush : Brushes.Transparent,
                    Background = isSelected ? Brushes.White : Brushes.Transparent,
                    Padding = new Thickness(10, 10, 6, 10),
                    Cursor = Cursors.Hand,
                    Child = stack
                };
                row.MouseLeftButtonUp += (sender, e) => SelectModule(module);

                SidebarPanel.Children.Add(row);
            }
        }

        private void SelectModule(LoadedModule module)
        {
            _selectedModule = module;
            BuildSidebar(Application.EnabledModules().ToList());
            BuildContent();
        }

        private void BuildContent()
        {
            ContentPanel.Children.Clear();

            foreach (string warning in Application.LoadWarnings)
                ContentPanel.Children.Add(BuildNote(warning, Brushes.DarkRed));

            if (_selectedModule == null)
            {
                string message = Application.AllModules.Count == 0
                    ? "No modules installed yet."
                    : "No modules enabled. Click the settings icon to turn some on.";
                ContentPanel.Children.Add(BuildNote(message, Brushes.Gray));
                return;
            }

            List<ToolGroup> groupsWithTools = _selectedModule.Groups
                .Where(g => g.Tools != null && g.Tools.Count > 0)
                .ToList();

            if (groupsWithTools.Count == 0)
            {
                ContentPanel.Children.Add(BuildNote("No tools available yet.", Brushes.Gray));
                return;
            }

            foreach (ToolGroup group in groupsWithTools)
                ContentPanel.Children.Add(BuildGroup(group));
        }

        private UIElement BuildGroup(ToolGroup group)
        {
            var groupPanel = new StackPanel { Margin = new Thickness(4, 6, 0, 0) };

            int number = 1;
            foreach (ToolItem tool in group.Tools)
            {
                groupPanel.Children.Add(BuildToolButton(tool, number));
                number++;
            }

            return new Expander
            {
                Header = group.Name,
                FontWeight = FontWeights.Medium,
                FontSize = 13,
                Foreground = NavyBrush,
                IsExpanded = true,
                Margin = new Thickness(0, 0, 0, 8),
                BorderBrush = RedBrush,
                BorderThickness = new Thickness(0, 0, 0, 1),
                Content = groupPanel
            };
        }

        private UIElement BuildToolButton(ToolItem tool, int number)
        {
            string tooltip = string.IsNullOrEmpty(tool.Version)
                ? tool.Tooltip
                : string.Format("{0} ({1})", tool.Tooltip, tool.Version);

            var content = new StackPanel { Orientation = Orientation.Horizontal };

            content.Children.Add(new TextBlock
            {
                Text = number.ToString(),
                FontWeight = FontWeights.Medium,
                FontSize = 12,
                Foreground = NavyBrush,
                Width = 16,
                VerticalAlignment = VerticalAlignment.Center
            });

            if (!string.IsNullOrEmpty(tool.IconGlyph))
            {
                content.Children.Add(new TextBlock
                {
                    Text = tool.IconGlyph,
                    FontFamily = IconFont,
                    FontSize = 15,
                    Foreground = NavyBrush,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 8, 0)
                });
            }

            content.Children.Add(new TextBlock
            {
                Text = tool.Label,
                FontSize = 13,
                VerticalAlignment = VerticalAlignment.Center
            });

            var button = new Button
            {
                Content = content,
                ToolTip = tooltip,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Padding = new Thickness(8, 7, 8, 7),
                Margin = new Thickness(0, 0, 0, 6),
                Tag = tool
            };
            button.Click += ToolButton_Click;
            return button;
        }

        private static TextBlock BuildNote(string text, Brush color)
        {
            return new TextBlock
            {
                Text = text,
                Foreground = color,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(2, 2, 2, 8)
            };
        }

        private void ToolButton_Click(object sender, RoutedEventArgs e)
        {
            var tool = ((Button)sender).Tag as ToolItem;
            tool?.Action?.Invoke();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SettingsWindow { Owner = this };
            if (dialog.ShowDialog() == true)
            {
                Application.Settings.Save();
                RefreshModules();
            }
        }
    }
}
