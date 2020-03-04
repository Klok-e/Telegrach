using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace DesktopFrontend.Views.Controls
{
    public class LoginCarousel : UserControl
    {
        public LoginCarousel()
        {
            InitializeComponent();
            var carousel = this.FindControl<Carousel>("carousel");
            this.FindControl<Button>("left").Click += (s, e) => carousel.Previous();
            this.FindControl<Button>("right").Click += (s, e) => carousel.Next();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}