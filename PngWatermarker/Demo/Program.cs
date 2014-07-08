using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PngWatermarker;
namespace Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            PNGFile file = new PNGFile(@"Flower_Original.png");
            PNGScrambler scrambler = new PNGScrambler(file, new byte[] { 1, 2, 3, 4 }, new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 });

            for (var x = 0; x < file.lines.Count ; x++)
            {
                for (var y = 0; y < file.lines[x].Length; y++)
                {
                    PNGPixel p = scrambler.GetPixel();

                    p.Red = (byte)~p.Red;
                    p.Green = (byte)~p.Green;
                    p.Blue = (byte)~p.Blue;
                }
            }
            file.SaveAs(@"Flower_New.png");
        }
    }
}
