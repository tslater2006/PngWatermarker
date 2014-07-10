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
            PNGFile file = new PNGFile(@"Flower_Original.png");

            CompositeWatermark comp = new CompositeWatermark();

            TextWatermark text1 = new TextWatermark("Text1");
            FileWatermark file1 = new FileWatermark(@"TestFile.txt");

            comp.AddWatermark(text1);
            comp.AddWatermark(file1);

            RijndaelManaged aes = new RijndaelManaged();

            EncryptedWatermark crypted = new EncryptedWatermark(comp, aes, "password");

            Watermarker.EmbedWatermark(file,crypted,"password","Flower_Composite.png");

            EncryptedWatermark comp2 = new EncryptedWatermark(aes,"password");

            PNGFile file2 = new PNGFile("Flower_Composite.png");

            Watermarker.ExtractWatermark(file2, comp2, "password");


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
            //var plainMark = new TextWatermark("test");
            var plainMark = new FileWatermark(@"TestFile.txt");

            RijndaelManaged aes = new RijndaelManaged();
            aes.Padding = PaddingMode.Zeros;

            var cryptMark = new EncryptedWatermark(plainMark, aes, "asdf");

            PNGFile file = new PNGFile(@"Flower_Original.png");

            Watermarker.EmbedWatermark(file, cryptMark, "password", "Flower_Crypted.png");

            file = new PNGFile(@"Flower_Crypted.png");

            aes = new RijndaelManaged();
            aes.Padding = PaddingMode.Zeros;

            cryptMark = new EncryptedWatermark(aes, "asdf");
            Watermarker.ExtractWatermark(file, cryptMark, "password");


        }

    }
}
