using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace PngWatermarker.Watermarks
{
    public class TextWatermark : Watermark
    {
        public const byte TYPE = 01;
        public string Text;
        public TextWatermark(String text)
        {
            this.Text = text;
        }

        public TextWatermark() { }

        public override byte[] GetBytes() {
            MemoryStream ms = new MemoryStream();
            ms.WriteByte(TYPE);

            byte[] textBytes = System.Text.Encoding.UTF8.GetBytes(Text);
            byte[] textLength = BitConverter.GetBytes(textBytes.Length);
            ms.Write(textLength, 0, textLength.Length);
            ms.Write(textBytes,0, textBytes.Length);

            return ms.ToArray();
        }

        public override bool LoadFromBytes(byte[] data)
        {
            string contents = System.Text.Encoding.UTF8.GetString(data);
            this.Text = contents;

            return true;
        }

        public override byte GetMarkType()
        {
            return TYPE;
        }
    }
}
