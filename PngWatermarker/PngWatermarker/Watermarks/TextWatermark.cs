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
        public static const int TYPE = 01;
        private string text;
        public TextWatermark(String text)
        {
            this.text = text;
        }

        public byte[] GetBytes() {
            MemoryStream ms = new MemoryStream();
            ms.WriteByte(TextWatermark.TYPE);

            byte[] textBytes = System.Text.Encoding.UTF8.GetBytes(text);
            byte[] textLength = BitConverter.GetBytes(textBytes.Length);
            ms.Write(textLength, 0, textLength.Length);
            ms.Write(textBytes,0, textBytes.Length);

            return ms.ToArray();
        }
    }
}
