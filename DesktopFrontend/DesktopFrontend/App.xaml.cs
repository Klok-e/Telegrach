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
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(GetConnection()),
                };
            }

            base.OnFrameworkInitializationCompleted();
        }

        private ServerConnection GetConnection()
        {
            return new ServerConnection("blah blah", 30303);
        }
    }
}