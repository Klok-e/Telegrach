using System.Data;
using System.Drawing;
using System.IO;
using Bitmap = Avalonia.Media.Imaging.Bitmap;

namespace DesktopFrontend.Models
{
    public enum FileType
    {
        Generic = 0,
        Image = 1,
        Sound = 2,
        Video = 3,
    }

    public class MediaFile
    {
        public FileType Type { get; }
        public string FilePath { get; }

        public MediaFile(FileType type, string path)
        {
            Type = type;
            FilePath = path;
        }

        public MediaFile(string extension, string path) :
            this(ExtToEnum(extension), path)
        {
        }

        public Bitmap Bitmap()
        {
            return new Bitmap(FilePath);
        }

        public FileStream Bytes()
        {
            return File.OpenRead(FilePath);
        }

        public static FileType ExtToEnum(string ext)
        {
            switch (ext)
            {
                case ".png":
                case ".jpg":
                    return FileType.Image;
                case ".mp4":
                    return FileType.Video;
                case ".mp3":
                    return FileType.Sound;
                default:
                    return FileType.Generic;
            }
        }
    }
}