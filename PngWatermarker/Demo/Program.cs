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
            PNGFile file = new PNGFile(@"C:\Users\User\Desktop\Flower_Original.png");
            for (var x = 0; x < file.lines.Count; x++)
            {
                for (var y = 0; y < file.lines[x].Length; y++)
                {
                    file.lines[x][y].Red = (byte)~file.lines[x][y].Red;
                }
            }
            file.SaveAs(@"C:\Users\User\Desktop\Flower_New.png");
        }
    }
}
