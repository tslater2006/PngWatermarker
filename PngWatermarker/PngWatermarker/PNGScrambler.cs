using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
namespace PngWatermarker
{
    /// <summary>
    /// PNGScrambler provides a mechanism for randomizing the pixels used to hold the watermark.
    /// </summary> 
    class PNGScrambler
    {
        PNGFile png;
        Rfc2898DeriveBytes bytes;

        private List<PixelCoord> coords = new List<PixelCoord>();

        public PNGScrambler(PNGFile png, byte[] key, byte[] salt)
        {
            
            this.png = png;
            bytes = new Rfc2898DeriveBytes(key, salt, 10);
            
            initCoords();
        }

        public void Reset()
        {
            bytes.Reset();
            initCoords();
        }
        private void initCoords()
        {
            coords.Clear();
            for (int x = 0; x < png.lines.Count;x++ )
            {
                for (int y = 0; y < png.lines[x].Length; y++)
                {
                    /* skip 0,0 thru 0,10 - for the salt */
                    /*if (x > 0 || y > 10)
                    {*/
                        PixelCoord pc = new PixelCoord() { x = x, y = y };

                        coords.Add(pc);
                    /*}*/
                    
                }
            }
        }

        public PNGPixel GetPixel()
        {
            int offset = Math.Abs(BitConverter.ToInt32(bytes.GetBytes(4),0)) % coords.Count;
            PixelCoord pix = coords[offset];

            coords.RemoveAt(offset);

            //Console.WriteLine("(" + pix.x + ", " + pix.y + ")");
            return png.lines[pix.x][pix.y];
            
        }

    }

    struct PixelCoord
    {
        public int x;
        public int y;
    }
}
