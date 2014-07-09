using PngWatermarker.Watermarks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace PngWatermarker
{
    public class Watermarker
    {
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
    }
}
