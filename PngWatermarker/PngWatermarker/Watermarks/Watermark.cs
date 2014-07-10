using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PngWatermarker.Watermarks
{
    public abstract class Watermark
    {
        public abstract byte[] GetBytes();
        internal abstract bool LoadFromBytes(byte[] data);
        internal abstract byte GetMarkType();
    }
}
