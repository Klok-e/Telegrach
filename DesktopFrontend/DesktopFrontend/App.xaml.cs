using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using DesktopFrontend.Models;
using DesktopFrontend.ViewModels;
using DesktopFrontend.Views;

namespace DesktopFrontend
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var storage = new DataStorage();
                var connection = new ServerConnection("127.0.0.1", 9999, storage);
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(connection, storage),
                };
                desktop.Exit += (o, a) => { connection.Dispose(); };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}