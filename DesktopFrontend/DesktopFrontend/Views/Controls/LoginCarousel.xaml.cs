using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using DynamicData.Binding;

namespace DesktopFrontend.Views.Controls
{
    public class LoginCarousel : UserControl
    {
        public static readonly DirectProperty<LoginCarousel, bool> IsSelectedFirstProperty =
            AvaloniaProperty.RegisterDirect<LoginCarousel, bool>(nameof(IsSelectedFirst),
                lo => lo.IsSelectedFirst);

        public static readonly DirectProperty<LoginCarousel, bool> IsSelectedLastProperty =
            AvaloniaProperty.RegisterDirect<LoginCarousel, bool>(nameof(IsSelectedLast),
                lo => lo.IsSelectedLast);

        private bool _isSelectedFirst;

        public bool IsSelectedFirst
        {
            get => _isSelectedFirst;
            private set => SetAndRaise(IsSelectedFirstProperty, ref _isSelectedFirst, value);
        }

        private bool _isSelectedLast;

        public bool IsSelectedLast
        {
            get => _isSelectedLast;
            private set => SetAndRaise(IsSelectedLastProperty, ref _isSelectedLast, value);
        }

        public LoginCarousel()
        {
            InitializeComponent();
            var carousel = this.FindControl<Carousel>("carousel");
            carousel.WhenPropertyChanged(c => c.SelectedIndex)
                .Subscribe(_ =>
                {
                    IsSelectedLast = carousel.SelectedIndex == carousel.ItemCount - 1;
                    IsSelectedFirst = carousel.SelectedIndex == 0;
                });
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}