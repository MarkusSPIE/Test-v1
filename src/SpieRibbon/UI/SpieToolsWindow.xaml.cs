using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SpieRibbon.Contracts;
using SpieRibbon.Core;

namespace SpieRibbon.UI
{
    public partial class SpieToolsWindow : Window
    {
        public SpieToolsWindow()
        {
            InitializeComponent();
            FooterText.Text = "Created by MVS - " + Application.VersionLabel;
        }

        /// <summary>Rebuilds the visible tree from the currently enabled modules.</summary>
        public void RefreshModules()
        {
            ModulesPanel.Children.Clear();

            foreach (string warning in Application.LoadWarnings)
                ModulesPanel.Children.Add(BuildNote(warning, Brushes.DarkRed));

            var enabled = Application.EnabledModules().ToList();

            if (enabled.Count == 0)
            {
                string message = Application.AllModules.Count == 0
                    ? "No modules installed yet."
                    : "No modules enabled. Click Settings to turn some on.";
                ModulesPanel.Children.Add(BuildNote(message, Brushes.Gray));
                return;
            }

            foreach (LoadedModule module in enabled)
                ModulesPanel.Children.Add(BuildCategory(module));
        }

        private UIElement BuildCategory(LoadedModule module)
        {
            var categoryPanel = new StackPanel();

            var groupsWithTools = module.Groups
                .Where(g => g.Tools != null && g.Tools.Count > 0)
                .ToList();

            if (groupsWithTools.Count == 0)
            {
                categoryPanel.Children.Add(BuildNote("No tools available yet.", Brushes.Gray));
            }
            else
            {
                foreach (ToolGroup group in groupsWithTools)
                    categoryPanel.Children.Add(BuildGroup(group));
            }

            return new Expander
            {
                Header = module.Name,
                FontWeight = FontWeights.Bold,
                IsExpanded = true,
                Margin = new Thickness(0, 0, 0, 8),
                Content = categoryPanel
            };
        }

        private UIElement BuildGroup(ToolGroup group)
        {
            var groupPanel = new StackPanel { Margin = new Thickness(4, 6, 0, 0) };

            foreach (ToolItem tool in group.Tools)
            {
                string tooltip = string.IsNullOrEmpty(tool.Version)
                    ? tool.Tooltip
                    : string.Format("{0} ({1})", tool.Tooltip, tool.Version);

                var button = new Button
                {
                    Content = tool.Label,
                    ToolTip = tooltip,
                    HorizontalContentAlignment = HorizontalAlignment.Left,
                    Padding = new Thickness(10, 8, 10, 8),
                    Margin = new Thickness(0, 0, 0, 6),
                    FontWeight = FontWeights.Normal,
                    Tag = tool
                };
                button.Click += ToolButton_Click;
                groupPanel.Children.Add(button);
            }

            return new Expander
            {
                Header = group.Name,
                FontWeight = FontWeights.Normal,
                IsExpanded = true,
                Margin = new Thickness(4, 0, 0, 4),
                Content = groupPanel
            };
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
