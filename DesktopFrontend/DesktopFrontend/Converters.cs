using System;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using DesktopFrontend.Models;

namespace DesktopFrontend
{
    public static class Converters
    {
        private static readonly Bitmap generic_image;
        private static readonly Bitmap generic_audio;
        private static readonly Bitmap generic_video;
        private static readonly Bitmap generic_file;

        static Converters()
        {
            var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();

            // TODO: this must use Resources instead of hardcoding filenames  
            generic_image = new Bitmap(assets.Open(new Uri($"avares://DesktopFrontend/Assets/generic_image.png")));
            generic_audio = new Bitmap(assets.Open(new Uri($"avares://DesktopFrontend/Assets/generic_audio.png")));
            generic_video = new Bitmap(assets.Open(new Uri($"avares://DesktopFrontend/Assets/generic_video.png")));
            generic_file = new Bitmap(assets.Open(new Uri($"avares://DesktopFrontend/Assets/generic_file.png")));
        }

        public static readonly IValueConverter FileToIcon =
            new FuncValueConverter<MediaFile, Bitmap>(f =>
            {
                return f.Type switch
                {
                    FileType.Generic => generic_file,
                    FileType.Sound => generic_audio,
                    FileType.Video => generic_video,
                    FileType.Image => new Bitmap(f.FilePath)
                };
            });
    }
}