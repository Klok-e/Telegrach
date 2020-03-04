using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace DesktopFrontend.Views.Controls
{
    public class LoginCarousel : UserControl
    {
        public LoginCarousel()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}