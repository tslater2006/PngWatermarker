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
        public abstract bool LoadFromBytes(byte[] data);
        public abstract byte GetMarkType();
    }
}
