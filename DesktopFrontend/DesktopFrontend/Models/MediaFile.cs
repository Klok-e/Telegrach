using System.Data;
using System.Drawing;
using System.IO;

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
        public byte[] Data { get; }

        public MediaFile()
        {
        }

        public Image Decode()
        {
            var stream = new MemoryStream(Data);
            Image.FromStream(stream);
            throw new RowNotInTableException();
        }
    }
}