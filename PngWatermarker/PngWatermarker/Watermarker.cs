using PngWatermarker.Watermarks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;

namespace PngWatermarker
{
    /// <summary>
    /// Main watermarking class.
    /// </summary>
    public class Watermarker
    {
        /// <summary>
        /// Embeds a given watermark into a PNG file and saves the result.
        /// </summary>
        /// <param name="file">PNGFile to use in the watermarking process.</param>
        /// <param name="mark">The watermark to embed.</param>
        /// <param name="password">A password for the embedding process</param>
        /// <param name="outputPath">Location of the saved file.</param>
        public static void EmbedWatermark(PNGFile file, Watermark mark, string password, string outputPath)
        {
            Rfc2898DeriveBytes bytes = new Rfc2898DeriveBytes(password, new byte[] { 112, 52, 63, 42, 180, 121, 53, 27 }, 1000);
            
            PNGScrambler scrambler = new PNGScrambler(file, bytes.GetBytes(16), bytes.GetBytes(8));
            
            EmbedData(mark.GetBytes(), scrambler);

            file.SaveAs(outputPath);
        }

        private static void EmbedData(byte[] data, PNGScrambler scrambler)
        {
            PNGPixel pix = null;
            
            List<byte> bits = BytesToBits(data);
            int channel = 3;

            for (var x = 0; x < bits.Count; x++)
            {
                if (channel == 3)
                {
                    pix = scrambler.GetPixel();
                    channel = 0;
                }

                switch(channel)
                {
                    case 0:
                        pix.Red &= 0xFC;
                        pix.Red |= bits[x];
                        break;
                    case 1:
                        pix.Green &= 0xFC;
                        pix.Green |= bits[x];
                        break;
                    case 2:
                        pix.Blue &= 0xFC;
                        pix.Blue |= bits[x];
                        break;
                }

                channel++;
            }
        }

        private static List<byte> BytesToBits(byte[] data)
        {
            List<byte> bits = new List<byte>();
            for (var x = 0; x < data.Length;x++)
            {
                bits.Add((byte)((data[x] & 0xC0) >> 6));
                bits.Add((byte)((data[x] & 0x30) >> 4));
                bits.Add((byte)((data[x] & 0x0C) >> 2));
                bits.Add((byte)((data[x] & 0x03)));
            }

            return bits;
        }

        private static byte[] BitsToBytes(List<byte> bits)
        {
            MemoryStream ms = new MemoryStream();
            for (var x = 0; x < bits.Count ; x += 4)
            {
                if (bits.Count - x >= 4){

                    byte b = (byte)(bits[x] << 6);
                    b |= (byte)(bits[x + 1] << 4);
                    b |= (byte)(bits[x + 2] << 2);
                    b |= (byte)(bits[x + 3]);


                    ms.WriteByte(b);
                }
            }

            return ms.ToArray();
        }

        private static byte[] ReadBytes(PNGFile file, PNGScrambler scrambler, int count, int skip = 0)
        {
            scrambler.Reset();

            List<byte> bits = new List<byte>();
            int numBitsToRead = count * 8;
            int pixelsToRead = numBitsToRead / 6 + ((numBitsToRead / 6.0)%1.0 != 0 ? 1 : 0) ;

            int pixelsToSkip = (skip * 8) / 6;
            //int bitPairsToThrowaway = pixelsToSkip == 0 ? 0 : (6 - ((skip * 8) % 6)) / 2;
            int bitPairsToThrowaway = ((skip * 8) % 6) / 2;

            if (bitPairsToThrowaway == 2) { pixelsToRead++; }

            for (var x = 0; x < pixelsToSkip; x++)
            {
                scrambler.GetPixel();
            }

            for (var x = 0; x < pixelsToRead; x++)
            {
                PNGPixel p = scrambler.GetPixel();
                bits.Add((byte)(p.Red & 0x03));
                bits.Add((byte)(p.Green & 0x03));
                bits.Add((byte)(p.Blue & 0x03));
            }

            for (var x = 0; x < bitPairsToThrowaway; x++)
            {
                bits.RemoveAt(0);
            }

           return BitsToBytes(bits);
        }

        /// <summary>
        /// Extracts a stored watermark from a PNGFile.
        /// </summary>
        /// <param name="file">PNGFile that contains the watermark.</param>
        /// <param name="mark">An empty watermark that will be populated.</param>
        /// <param name="password">Password that was used to embed the watermark.</param>
        /// <returns></returns>
        public static bool ExtractWatermark(PNGFile file, Watermark mark, string password)
        {
            Rfc2898DeriveBytes bytes = new Rfc2898DeriveBytes(password, new byte[] { 112, 52, 63, 42, 180, 121, 53, 27 }, 1000);

            PNGScrambler scrambler = new PNGScrambler(file, bytes.GetBytes(16), bytes.GetBytes(8));

            byte[] type = ReadBytes(file, scrambler, 1);

            if (type[0] != mark.GetMarkType()) { return false; }

            byte[] dword = ReadBytes(file, scrambler, 4, 1);
            int length = BitConverter.ToInt32(dword, 0);

            byte[] data = ReadBytes(file, scrambler, length, 5);

            mark.LoadFromBytes(data);
            return true;

        }
    }
}
