using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PngWatermarker.Watermarks
{
    public abstract class Watermark
    {
        internal abstract byte[] GetBytes();
        internal abstract byte GetMarkType();
    }
}
