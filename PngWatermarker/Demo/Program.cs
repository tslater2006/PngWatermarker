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
            byte[] bigData = new byte[312721 - 1 - 6];
            for (var x = 0; x < bigData.Length;x++)
            {
                bigData[x] = 255;
            }
            Watermarker.EmbedWatermark(file, new BinaryWatermark(bigData), "password", "Flower_BinMark.png");

            PNGFile extractFile = new PNGFile("Flower_Textmark.png");

            Watermark mark = Watermarker.ExtractWatermark(extractFile, "password");

            extractFile = new PNGFile("Flower_BinMark.png");
            mark = Watermarker.ExtractWatermark(extractFile, "password");

            Console.ReadKey();
        }
    }
}
