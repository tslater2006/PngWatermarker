using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PngWatermarker;
using PngWatermarker.Watermarks;
namespace Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            PNGFile file = new PNGFile(@"Flower_Original.png");

            Console.WriteLine("This PNG can hold: " + file.EstimatedStorage + " bytes of data");

            Watermarker.EmbedWatermark(file, new TextWatermark("test"), "password", "Flower_TextMark.png");
            Watermarker.EmbedWatermark(file, new BinaryWatermark(new byte[312713 -1 - 4]), "password", "Flower_BinMark.png");
            
            Console.ReadKey();
        }
    }
}
