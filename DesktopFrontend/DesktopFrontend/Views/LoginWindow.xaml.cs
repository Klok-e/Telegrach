using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace DesktopFrontend.Views
{
    public class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);            
        }
        
    }
}