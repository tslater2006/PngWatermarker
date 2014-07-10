using System;
using NUnit.Framework;
using System.Security.Cryptography;
using PngWatermarker;
using PngWatermarker.Watermarks;
using System.IO;
namespace Tests
{
    [TestFixture]
    public class UnitTests : AssertionHelper
    {
        private PNGFile file;
        private RijndaelManaged aes;

        [SetUp]
        protected void SetUp()
        {
            if (Directory.Exists("results") == false){
                Directory.CreateDirectory("results");
            }
            file = new PNGFile(@"TestImage.png");
            aes = new RijndaelManaged();
            aes.Padding = PaddingMode.Zeros;
            aes.KeySize = 256;
        }

        [Test]
        public void TestStorageCalculation()
        {
            int size = file.EstimatedStorage;
            byte[] data = new byte[size];
            for (var x = 0; x < size; x++)
            {
                data[x] = 0xFF;
            }

            Watermarker.EmbedWatermark(file,new BinaryWatermark(data),"password","results/StorageCalc.png");

            Assert.That(StoreTooMuch, Throws.Exception);
        }

        [Test]
        public void TestScrambling()
        {
            TextWatermark mark = new TextWatermark("This is a test");

            // Length of 19 for 5 byte header and 14 byte string 
            Expect(mark.GetBytes().Length, Is.EqualTo(19));

            Watermarker.EmbedWatermark(file, mark, "password", "results/Scrambling.png");

            TextWatermark extract = new TextWatermark();
            PNGFile file2 = new PNGFile("results/TextMark.png");

            bool result = Watermarker.ExtractWatermark(file2, extract, "password");

            Expect(result, Is.EqualTo(true));
                        

            result = Watermarker.ExtractWatermark(file2, extract, "foobar");

            Expect(result, Is.EqualTo(false));
        }

        [Test]
        public void TestTextWatermark()
        {
            TextWatermark mark = new TextWatermark("This is a test");

            // Length of 19 for 5 byte header and 14 byte string 
            Expect(mark.GetBytes().Length, Is.EqualTo(19));

            Watermarker.EmbedWatermark(file, mark, "password", "results/TextMark.png");

            TextWatermark extract = new TextWatermark();
            PNGFile file2 = new PNGFile("results/TextMark.png");

            bool result = Watermarker.ExtractWatermark(file2, extract, "password");

            Expect(result, Is.EqualTo(true));

            Expect(extract.Text, Is.EqualTo("This is a test"));

        }

        [Test]
        public void TestBinaryWatermark()
        {
            BinaryWatermark mark = new BinaryWatermark(new byte[] { 1, 2, 3, 4 });

            // Length of 9 for 5 byte header and 4 byte data 
            Expect(mark.GetBytes().Length, Is.EqualTo(9));

            Watermarker.EmbedWatermark(file, mark, "password", "results/BinaryMark.png");

            BinaryWatermark extract = new BinaryWatermark();
            PNGFile file2 = new PNGFile("results/BinaryMark.png");

            bool result = Watermarker.ExtractWatermark(file2, extract, "password");

            Expect(result, Is.EqualTo(true));

            Expect(extract.data, Is.EqualTo(new byte[] { 1, 2, 3, 4 }));

        }

        [Test]
        public void TestFileWatermark()
        {
            FileWatermark mark = new FileWatermark(@"TestFile.txt");

            // Length of  for 5 byte header and 4 byte extension length, 4 byte extension, 4 byte file length, 20 byte file data 
            Expect(mark.GetBytes().Length, Is.EqualTo(37));

            Watermarker.EmbedWatermark(file, mark, "password", "results/FileMark.png");

            FileWatermark extract = new FileWatermark();
            PNGFile file2 = new PNGFile("results/FileMark.png");

            bool result = Watermarker.ExtractWatermark(file2, extract, "password");

            Expect(result, Is.EqualTo(true));

            Expect(extract.extension, Is.EqualTo(".txt"));
            Expect(extract.fileData, Is.EqualTo(System.Text.Encoding.UTF8.GetBytes("This is a test file!")));

        }
        
        [Test]
        public void TestCompositeWatermark()
        {
            CompositeWatermark comp = new CompositeWatermark();
            TextWatermark mark1 = new TextWatermark("This is mark #1");
            TextWatermark mark2 = new TextWatermark("This is mark #2");

            EncryptedWatermark enc = new EncryptedWatermark(mark1, aes, "supersecret");

            comp.AddWatermark(mark1);
            comp.AddWatermark(mark2);
            Expect(comp.GetWatermarkCount(), Is.EqualTo(2));

            // Cannot add encrypted watermarks to a composite
            comp.AddWatermark(enc);
            Expect(comp.GetWatermarkCount(), Is.EqualTo(2));

            Watermarker.EmbedWatermark(file, comp, "password", "results/CompositeMark.png");

            CompositeWatermark extract = new CompositeWatermark();
            PNGFile file2 = new PNGFile("results/CompositeMark.png");

            bool result = Watermarker.ExtractWatermark(file2, extract, "password");

            Expect(result, Is.EqualTo(true));

            Watermark[] marks = extract.GetWatermarks();

            Expect(marks.Length, Is.EqualTo(2));

            Assert.IsInstanceOf<TextWatermark>(marks[0]);

            Assert.IsInstanceOf<TextWatermark>(marks[1]);

            Expect(((TextWatermark)marks[0]).Text, Is.EqualTo("This is mark #1"));
            Expect(((TextWatermark)marks[1]).Text, Is.EqualTo("This is mark #2"));
        }

        [Test]
        public void TestEncryptedWatermark()
        {
            TextWatermark mark = new TextWatermark("This should be encrypted");
            EncryptedWatermark encrypted = new EncryptedWatermark(mark, aes, "super-secret");

            Watermarker.EmbedWatermark(file, encrypted, "password", "results/EncryptedMark.png");

            EncryptedWatermark extract = new EncryptedWatermark(aes, "super-secret");
            PNGFile file2 = new PNGFile("results/EncryptedMark.png");

            Watermarker.ExtractWatermark(file2, extract, "password");

            Assert.IsInstanceOf<TextWatermark>(extract.DecryptedMark);
            Expect(((TextWatermark)extract.DecryptedMark).Text, Is.EqualTo("This should be encrypted"));

        }

        private void StoreTooMuch()
        {
            int size = file.EstimatedStorage + 1;
            byte[] data = new byte[size];
            for (var x = 0; x < size; x++)
            {
                data[x] = 0xFF;
            }

            Watermarker.EmbedWatermark(file, new BinaryWatermark(data), "password", "results/StorageCalc.png");

        }
    }
}
