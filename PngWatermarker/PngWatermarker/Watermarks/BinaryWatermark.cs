using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace PngWatermarker.Watermarks
{
    /// <summary>
    /// Watermark that stores only binary data.
    /// </summary>
    public class BinaryWatermark : Watermark
    {

        /// <summary>
        /// Watermark TYPE, used by the watermarking engine at extraction time.
        /// </summary>
        public const int TYPE = 03;

        /// <summary>
        /// Data that this watermark holds.
        /// </summary>
        public byte[] data;
        
        /// <summary>
        /// Constructor to be used when embedding a watermark.
        /// </summary>
        /// <param name="data"></param>
        public BinaryWatermark(byte[] data)
        {
            this.data = data;
        }

        /// <summary>
        /// Constructor to be used when extracting a watermark of this type.
        /// </summary>
        public BinaryWatermark() { }

        /// <summary>
        /// Converts this watermark into a byte array.
        /// </summary>
        /// <returns>Byte array </returns>
        public override byte[] GetBytes()
        {
            MemoryStream ms = new MemoryStream();
            ms.WriteByte(TYPE);

            byte[] length = BitConverter.GetBytes(data.Length);
            ms.Write(length, 0, length.Length);
            ms.Write(data, 0, data.Length);

            return ms.ToArray();
        }

        internal override bool LoadFromBytes(byte[] data)
        {
            this.data = data;
            return true;
        }

        internal override byte GetMarkType()
        {
            return TYPE;
        }
    }
}
