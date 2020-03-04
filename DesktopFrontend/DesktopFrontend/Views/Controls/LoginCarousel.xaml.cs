using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace DesktopFrontend.Views.Controls
{
    public class LoginCarousel : UserControl
    {
        private Carousel _carousel;
        private Button _left;
        private Button _right;
        

        public LoginCarousel()
        {
            this.InitializeComponent();
            _left.Click += (s, e) => _carousel.Previous();
            _right.Click += (s, e) => _carousel.Next();
           
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            _carousel = this.FindControl<Carousel>("carousel");
            _left = this.FindControl<Button>("left");
            _right = this.FindControl<Button>("right");
           
        }        
    }
}