// =============================================
// ELITE LAN CENTER - INPUT DIALOG
// =============================================

using System.Windows;
using System.Windows.Controls;

namespace EliteLanCenter.Views
{
    public class InputDialog : Window
    {
        private readonly TextBox _txtInput;
        public string InputValue => _txtInput.Text;

        public InputDialog(string title, string prompt, string defaultValue = "")
        {
            Title = title;
            Width = 400;
            Height = 180;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ResizeMode = ResizeMode.NoResize;
            WindowStyle = WindowStyle.ToolWindow;
            Background = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(0x1A, 0x1F, 0x35));

            var grid = new Grid { Margin = new Thickness(20) };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var lblPrompt = new TextBlock
            {
                Text = prompt,
                Foreground = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(0xE8, 0xEA, 0xF6)),
                FontSize = 13,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 12)
            };
            Grid.SetRow(lblPrompt, 0);

            _txtInput = new TextBox
            {
                Text = defaultValue,
                Height = 36,
                Padding = new Thickness(10, 0, 10, 0),
                FontSize = 13,
                Background = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(0x2D, 0x32, 0x50)),
                Foreground = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(0xE8, 0xEA, 0xF6)),
                CaretBrush = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(0x01, 0xEF, 0xAC)),
                BorderThickness = new Thickness(0),
                VerticalContentAlignment = VerticalAlignment.Center
            };
            Grid.SetRow(_txtInput, 1);

            var panelBotones = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 16, 0, 0)
            };
            Grid.SetRow(panelBotones, 2);

            var btnAceptar = new Button
            {
                Content = "Aceptar",
                Width = 90,
                Height = 32,
                Margin = new Thickness(0, 0, 8, 0),
                Background = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(0x01, 0xEF, 0xAC)),
                Foreground = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(0x11, 0x18, 0x27)),
                BorderThickness = new Thickness(0),
                Cursor = System.Windows.Input.Cursors.Hand
            };
            btnAceptar.Click += (s, e) => { DialogResult = true; Close(); };

            var btnCancelar = new Button
            {
                Content = "Cancelar",
                Width = 90,
                Height = 32,
                Background = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(0x2D, 0x32, 0x50)),
                Foreground = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(0xE8, 0xEA, 0xF6)),
                BorderThickness = new Thickness(0),
                Cursor = System.Windows.Input.Cursors.Hand
            };
            btnCancelar.Click += (s, e) => { DialogResult = false; Close(); };

            panelBotones.Children.Add(btnAceptar);
            panelBotones.Children.Add(btnCancelar);

            grid.Children.Add(lblPrompt);
            grid.Children.Add(_txtInput);
            grid.Children.Add(panelBotones);

            Content = grid;

            Loaded += (s, e) =>
            {
                _txtInput.Focus();
                _txtInput.SelectAll();
            };
        }
    }
}
