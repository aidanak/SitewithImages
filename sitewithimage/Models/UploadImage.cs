using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace sitewithimage.Models
{
    public class UploadImage
    {
        public UploadImage()
        {
            Thumbnails = new List<Thumbnail> { new Thumbnail { Width = 200, Height = 300 } };
        }
        public string Name { get; set; }
        public string ContentType { get; set; }
        public byte[] Data { get; set; }
        public string Url { get; set; }
        public List<Thumbnail> Thumbnails { get; set; }
    }
    public class Thumbnail
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public string Url { get; set; }
    }
}