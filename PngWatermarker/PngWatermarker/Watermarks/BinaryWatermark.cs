using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace PngWatermarker.Watermarks
{
    public class BinaryWatermark : Watermark
    {
        public static const int TYPE = 03;

        public byte[] data;
        
        public BinaryWatermark(byte[] data)
        {
            this.data = data;
        }


        public override byte[] GetBytes()
        {
            MemoryStream ms = new MemoryStream();
            ms.WriteByte(FileWatermark.TYPE);

            byte[] length = BitConverter.GetBytes(data.Length);
            ms.Write(length, 0, length.Length);
            ms.Write(data, 0, data.Length);

            return ms.ToArray();
        }
    }
}
