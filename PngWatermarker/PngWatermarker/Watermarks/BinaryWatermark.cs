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
        public const int TYPE = 03;

        public byte[] data;
        
        public BinaryWatermark(byte[] data)
        {
            this.data = data;
        }

        public BinaryWatermark() { }

        public override byte[] GetBytes()
        {
            MemoryStream ms = new MemoryStream();
            ms.WriteByte(TYPE);

            byte[] length = BitConverter.GetBytes(data.Length);
            ms.Write(length, 0, length.Length);
            ms.Write(data, 0, data.Length);

            return ms.ToArray();
        }

        public override bool LoadFromBytes(byte[] data)
        {
            this.data = data;
            return true;
        }

        public override byte GetMarkType()
        {
            return TYPE;
        }
    }
}
