using EliteLanCenter.Helpers;
using System;
using System.IO;
using System.Windows;

namespace EliteLanCenter
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ThemeManager.Inicializar();

            AppDomain.CurrentDomain.UnhandledException += (s, args) =>
            {
                var ex = args.ExceptionObject as Exception;
                LogError("UnhandledException", ex);
            };

            DispatcherUnhandledException += (s, args) =>
            {
                LogError("DispatcherUnhandledException", args.Exception);
                MessageBox.Show($"Error no manejado:\n{args.Exception.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                args.Handled = true;
            };
        }

        private void LogError(string source, Exception? ex)
        {
            var logFile = Path.Combine(Path.GetTempPath(), "elite_error.log");
            var msg = $"[{DateTime.Now}] {source}: {ex?.Message}\n{ex?.StackTrace}\n";
            try { File.AppendAllText(logFile, msg); } catch { }
        }
    }
}
