using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Shell;

namespace SpieRibbon.Chrome
{
    /// <summary>
    /// Applies the standard SPIE Ribbon custom title bar to a window: hides the native OS
    /// chrome, replaces it with a navy caption bar (red accent dot + title) and hand-built
    /// minimize/maximize/close buttons. Every window in the plugin should call this - see
    /// "Every window, not just the main toolbox" in DESIGN-SYSTEM.md.
    ///
    /// Usage: give the window's XAML an empty top-docked Border (e.g. x:Name="TitleBarHost"),
    /// then call SpieChrome.Apply(this, TitleBarHost, "Window Title") after InitializeComponent.
    /// The returned StackPanel is the right-aligned button row, in case a caller needs to insert
    /// its own extra button (e.g. Settings) before the minimize/maximize/close group.
    /// </summary>
    public static class SpieChrome
    {
        private const double CaptionHeight = 40;
        private static readonly FontFamily IconFont = new FontFamily("Segoe MDL2 Assets");

        public static StackPanel Apply(Window window, Border titleBarHost, string title)
        {
            WindowChrome.SetWindowChrome(window, new WindowChrome
            {
                CaptionHeight = CaptionHeight,
                ResizeBorderThickness = new Thickness(6),
                GlassFrameThickness = new Thickness(0),
                CornerRadius = new CornerRadius(0)
            });
            window.WindowStyle = WindowStyle.None;
            window.Background = Brushes.White;

            titleBarHost.Background = SpieColors.Navy;
            titleBarHost.Height = CaptionHeight;
            titleBarHost.Padding = new Thickness(14, 0, 6, 0);

            var dock = new DockPanel { LastChildFill = false };

            var left = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
            left.Children.Add(new Ellipse
            {
                Width = 14,
                Height = 14,
                Fill = SpieColors.Red,
                Margin = new Thickness(0, 0, 10, 0),
                VerticalAlignment = VerticalAlignment.Center
            });
            left.Children.Add(new TextBlock
            {
                Text = title,
                Foreground = Brushes.White,
                FontWeight = FontWeights.Medium,
                FontSize = 14,
                VerticalAlignment = VerticalAlignment.Center
            });
            dock.Children.Add(left);

            var right = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
            WindowChrome.SetIsHitTestVisibleInChrome(right, true);
            DockPanel.SetDock(right, Dock.Right);
            dock.Children.Add(right);

            titleBarHost.Child = dock;

            Button minimizeButton = MakeButton(ChromeIcons.Minimize, () => window.WindowState = WindowState.Minimized, false);
            Button maximizeButton = MakeButton(ChromeIcons.Maximize, () => ToggleMaximize(window), false);
            Button closeButton = MakeButton(ChromeIcons.Close, window.Close, true);

            window.StateChanged += (sender, e) =>
            {
                ((TextBlock)maximizeButton.Content).Text =
                    window.WindowState == WindowState.Maximized ? ChromeIcons.Restore : ChromeIcons.Maximize;
            };

            right.Children.Add(minimizeButton);
            right.Children.Add(maximizeButton);
            right.Children.Add(closeButton);

            return right;
        }

        private static void ToggleMaximize(Window window)
        {
            window.WindowState = window.WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }

        private static Button MakeButton(string glyph, Action onClick, bool hoverRed)
        {
            var glyphText = new TextBlock
            {
                Text = glyph,
                FontFamily = IconFont,
                FontSize = 10,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            var button = new Button
            {
                Content = glyphText,
                Width = 32,
                Height = CaptionHeight,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand,
                Padding = new Thickness(0)
            };

            Brush hoverBrush = hoverRed
                ? new SolidColorBrush(Color.FromRgb(0xE8, 0x11, 0x23))
                : new SolidColorBrush(Color.FromArgb(40, 255, 255, 255));

            button.MouseEnter += (sender, e) => button.Background = hoverBrush;
            button.MouseLeave += (sender, e) => button.Background = Brushes.Transparent;
            button.Click += (sender, e) => onClick();

            return button;
        }
    }
}
