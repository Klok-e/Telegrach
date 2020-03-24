using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace DesktopFrontend.Views
{
    public class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // To fix a bug on linux where Height and Width are NaN and ListBox doesn't
            // want to show scrollbar until resize of the window
            var size = ClientSize;
            Height = size.Height + 50; //50 extra pixels makes pic's comments visible
            Width = size.Width;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

    }
}