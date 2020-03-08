using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace DesktopFrontend.Views
{
    public class LoginCredentialsView : UserControl
    {
        public LoginCredentialsView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}