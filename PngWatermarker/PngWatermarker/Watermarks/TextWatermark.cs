using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace PngWatermarker.Watermarks
{
    /// <summary>
    /// A Textual Watermark.
    /// </summary>
    public class TextWatermark : Watermark
    {
        public const byte TYPE = 01;
        public string Text;

        /// <summary>
        /// Constructor for basic Text Watermark
        /// </summary>
        /// <param name="text">The string this watermark should hold.</param>
        public TextWatermark(String text)
        {
            this.Text = text;
        }

        internal override byte[] GetBytes() {
            MemoryStream ms = new MemoryStream();
            ms.WriteByte(TYPE);

            byte[] textBytes = System.Text.Encoding.UTF8.GetBytes(Text);
            byte[] textLength = BitConverter.GetBytes(textBytes.Length);
            ms.Write(textLength, 0, textLength.Length);
            ms.Write(textBytes,0, textBytes.Length);

            return ms.ToArray();
        }

        internal static TextWatermark LoadFromBytes(byte[] data)
        {
            string contents = System.Text.Encoding.UTF8.GetString(data);

            return new TextWatermark(contents);

        }

        internal override byte GetMarkType()
        {
            return TYPE;
        }
    }
}
