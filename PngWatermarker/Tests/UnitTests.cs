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
            //Storage estimates are only valid if ReedSolomon isn't being used.
            Watermarker.ReedSolomonProtection = false;

            Watermarker.EmbedWatermark(file,new BinaryWatermark(data),"password","results/StorageCalc.png");

            Assert.That(StoreTooMuch, Throws.Exception);
        }
        [Test]
        public void TestSRStorageCalculation()
        {
            int size = file.EstimatedSolomonReedStorage;

            byte[] data = new byte[size];
            for (var x = 0; x < size; x++)
            {
                data[x] = 0xFF;
            }

            //Storage estimates are only valid if ReedSolomon isn't being used.
            Watermarker.ReedSolomonProtection = true;

            Watermarker.EmbedWatermark(file, new BinaryWatermark(data), "password", "results/StorageCalc.png");

            Assert.That(StoreTooMuchSR, Throws.Exception);
        }
        [Test]
        public void TestScrambling()
        {
            TextWatermark mark = new TextWatermark("This is a test");


            Watermarker.EmbedWatermark(file, mark, "password", "results/Scrambling.png");

            
            PNGFile file2 = new PNGFile("results/TextMark.png");

            TextWatermark extracted = Watermarker.ExtractWatermark <TextWatermark>(file2, "password");


            extracted = Watermarker.ExtractWatermark <TextWatermark>(file2, "foobar");

            Expect(extracted, Is.Null);
        }

        [Test]
        public void TestDoubleEncrypt()
        {
            EncryptedWatermark.Algorithm = aes;

            TextWatermark mark = new TextWatermark("This will be encrypted twice");
            EncryptedWatermark enc1 = new EncryptedWatermark(mark, "password1");
            EncryptedWatermark enc2 = new EncryptedWatermark(enc1, "password2");

            Watermarker.EmbedWatermark(file, enc2, "password", "results/DoubleEncrypt.png");

            PNGFile file2 = new PNGFile("results/DoubleEncrypt.png");

            EncryptedWatermark extract = Watermarker.ExtractWatermark <EncryptedWatermark>(file, "password");
            extract = extract.Decrypt<EncryptedWatermark>("password2");

            
            mark = extract.Decrypt<TextWatermark>("password1");

            Expect(mark.Text, Is.EqualTo("This will be encrypted twice"));

        }

        [Test]
        public void TestTextWatermark()
        {
            TextWatermark mark = new TextWatermark("This is a test");

            Watermarker.EmbedWatermark(file, mark, "password", "results/TextMark.png");

            PNGFile file2 = new PNGFile("results/TextMark.png");

            TextWatermark extract = Watermarker.ExtractWatermark<TextWatermark>(file2, "password");

            Expect(extract.Text, Is.EqualTo("This is a test"));

        }

        [Test]
        public void TestRSTextWatermark()
        {
            TextWatermark mark = new TextWatermark("This is a test");

            Watermarker.ReedSolomonProtection = true;

            Watermarker.EmbedWatermark(file, mark, "password", "results/TextMark.png");

            PNGFile file2 = new PNGFile("results/TextMark.png");

            TextWatermark extract = Watermarker.ExtractWatermark<TextWatermark>(file2, "password");

            Expect(extract.Text, Is.EqualTo("This is a test"));

        }

        [Test]
        public void TestLongRSWatermark()
        {
            byte[] data = new byte[300];
            for (var x = 0; x < 300; x++) { data[x] = (byte)(x % 17); }

            BinaryWatermark binMark = new BinaryWatermark(data);

            Watermarker.ReedSolomonProtection = true;

            Watermarker.EmbedWatermark(file, binMark, "password", "results/LongRS.png");

            PNGFile file2 = new PNGFile("results/LongRS.png");

            BinaryWatermark binMark2 = Watermarker.ExtractWatermark<BinaryWatermark>(file2, "password");

            for(var x = 0; x < binMark2.data.Length; x++)
            {
                Expect(binMark2.data[x], Is.EqualTo(binMark.data[x]));
            }

        }

        [Test]
        public void TestBinaryWatermark()
        {
            BinaryWatermark mark = new BinaryWatermark(new byte[] { 1, 2, 3, 4 });

            Watermarker.EmbedWatermark(file, mark, "password", "results/BinaryMark.png");

            PNGFile file2 = new PNGFile("results/BinaryMark.png");

            BinaryWatermark extract = (BinaryWatermark) Watermarker.ExtractWatermark(file2, "password");

            Expect(extract.data, Is.EqualTo(new byte[] { 1, 2, 3, 4 }));

        }

        [Test]
        public void TestFileWatermark()
        {
            FileWatermark mark = new FileWatermark(@"TestFile.txt");

            Watermarker.EmbedWatermark(file, mark, "password", "results/FileMark.png");

            
            PNGFile file2 = new PNGFile("results/FileMark.png");

            FileWatermark extract = (FileWatermark)Watermarker.ExtractWatermark(file2, "password");

            Expect(extract.extension, Is.EqualTo(".txt"));
            Expect(extract.fileData, Is.EqualTo(System.Text.Encoding.UTF8.GetBytes("This is a test file!")));

        }
        
        [Test]
        public void TestCompositeWatermark()
        {
            EncryptedWatermark.Algorithm = aes;

            CompositeWatermark comp = new CompositeWatermark();
            TextWatermark mark1 = new TextWatermark("This is mark #1");
            TextWatermark mark2 = new TextWatermark("This is mark #2");

            EncryptedWatermark enc = new EncryptedWatermark(mark1, "supersecret");

            comp.AddWatermark(mark1);
            comp.AddWatermark(mark2);
            Expect(comp.Watermarks.Count, Is.EqualTo(2));

            comp.AddWatermark(enc);
            Expect(comp.Watermarks.Count, Is.EqualTo(3));

            Watermarker.EmbedWatermark(file, comp, "password", "results/CompositeMark.png");

            
            PNGFile file2 = new PNGFile("results/CompositeMark.png");

            CompositeWatermark extract = (CompositeWatermark)Watermarker.ExtractWatermark(file2, "password");


            System.Collections.Generic.List<Watermark> marks = extract.Watermarks;

            Expect(marks.Count, Is.EqualTo(3));

            Assert.IsInstanceOf<TextWatermark>(marks[0]);

            Assert.IsInstanceOf<TextWatermark>(marks[1]);

            Assert.IsInstanceOf<EncryptedWatermark>(marks[2]);

            

            ((EncryptedWatermark)marks[2]).Decrypt("supersecret");

            Expect(((TextWatermark)marks[0]).Text, Is.EqualTo("This is mark #1"));
            Expect(((TextWatermark)marks[1]).Text, Is.EqualTo("This is mark #2"));
            Expect(((EncryptedWatermark)marks[2]).Decrypt<TextWatermark>("supersecret").Text, Is.EqualTo("This is mark #1"));
        }
        
        [Category("QuickTests")]
        [Test]
        public void TestEncryptedWatermark()
        {
            EncryptedWatermark.Algorithm = aes;

            TextWatermark mark = new TextWatermark("This should be encrypted");
            EncryptedWatermark encrypted = new EncryptedWatermark(mark, "super-secret");

            Watermarker.EmbedWatermark(file, encrypted, "password", "results/EncryptedMark.png");

            
            PNGFile file2 = new PNGFile("results/EncryptedMark.png");
            EncryptedWatermark extract = (EncryptedWatermark)Watermarker.ExtractWatermark(file2, "password");
            mark = extract.Decrypt<TextWatermark>("super-secret");

            Expect(mark, Is.Not.Null);

            Expect(mark.Text, Is.EqualTo("This should be encrypted"));
        }

        [Test]
        public void TestSeperateMarks()
        {
            // This will only pass if the marks do not overlap on any pixels.

            TextWatermark text1 = new TextWatermark("text1");
            TextWatermark text2 = new TextWatermark("text2");

            Watermarker.EmbedWatermark(file, text1, "password", "results/Embed1.png");

            file = new PNGFile("results/Embed1.png");

            Watermarker.EmbedWatermark(file, text2, "foobar", "results/Embed2Marks.png");

            PNGFile file2 = new PNGFile("results/Embed2Marks.png");

            TextWatermark extract1 = (TextWatermark)Watermarker.ExtractWatermark(file2, "password");

            TextWatermark extract2 = (TextWatermark)Watermarker.ExtractWatermark(file2, "foobar");

            Expect(extract1.Text, Is.EqualTo("text1"));
            Expect(extract2.Text, Is.EqualTo("text2"));

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

        private void StoreTooMuchSR()
        {
            int size = file.EstimatedSolomonReedStorage + 1;
            byte[] data = new byte[size];
            for (var x = 0; x < size; x++)
            {
                data[x] = 0xFF;
            }

            Watermarker.EmbedWatermark(file, new BinaryWatermark(data), "password", "results/StorageCalc.png");

        }
    }
}
