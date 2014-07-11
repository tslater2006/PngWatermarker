using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PngWatermarker;
using PngWatermarker.Watermarks;
using System.Security.Cryptography;
namespace Demo
{
    class Program
    {
        static void Main(string[] args)
        {



            /*Watermarker.EmbedWatermark(file, new TextWatermark("test"), "password", "Flower_TextMark.png");

            Watermarker.EmbedWatermark(file, new BinaryWatermark(new byte[100]), "password", "Flower_BinMark.png");

            Watermarker.EmbedWatermark(file, new FileWatermark(@"TestFile.txt"), "password", "Flower_FileMark.png");

            PNGFile extractFile = new PNGFile("Flower_Textmark.png");

            Watermark mark = Watermarker.ExtractWatermark(extractFile, "password");

            extractFile = new PNGFile("Flower_BinMark.png");
            mark = Watermarker.ExtractWatermark(extractFile, "password");

            extractFile = new PNGFile("Flower_FileMark.png");
            mark = Watermarker.ExtractWatermark(extractFile, "password");

            ((FileWatermark)mark).Save("ExtractedMark");*/

            //TestCryptoMarks();
            Console.WriteLine("Done!");
            Console.ReadKey();

        }

        private static void TestCryptoMarks()
        {



        }

    }
}
