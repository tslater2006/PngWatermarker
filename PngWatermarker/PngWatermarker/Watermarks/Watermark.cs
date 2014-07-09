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

        public static Type GetWatermarkType(int type)
        {
            switch(type)
            {
                case 1:
                    return typeof (TextWatermark);
                case 2:
                    return typeof(FileWatermark);
                case 3:
                    return typeof(BinaryWatermark);
            }
            return null;
        }


    }
}
