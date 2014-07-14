using PngWatermarker.Watermarks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;
using ZXing.Common.ReedSolomon;

namespace PngWatermarker
{
    /// <summary>
    /// Main watermarking class.
    /// </summary>
    public class Watermarker
    {
        public static bool ReedSolomonProtection = false;
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

            byte[] markBytes = mark.GetBytes();

            if (ReedSolomonProtection == false)
            {
                EmbedData(markBytes, scrambler);
            }
            else
            {
                markBytes = EncodeWithReedSolomon(markBytes);
                EmbedData(markBytes, scrambler);
            }

            file.SaveAs(outputPath);
        }

        private static byte[] EncodeWithReedSolomon(byte[] markBytes)
        {
            MemoryStream msOut = new MemoryStream();
            MemoryStream msIn = new MemoryStream(markBytes);

            byte[] rsType = null;
            byte[] rsSize = null;

            int numBlocks = (markBytes.Length / 256) + (markBytes.Length % 256 > 0 ? 1 : 0);

            // encode Type w/ ReedSolomon
            byte type = (byte)msIn.ReadByte();
            type |= 0x80;

            rsType = makeCodeWords(new byte[] { type }, 2);

            msOut.Write(rsType,0,rsType.Length);

            // encode Length w/ ReedSolomon
            byte[] size = new byte[4];
            msIn.Read(size, 0, 4);
            rsSize = makeCodeWords(size, 8);

            msOut.Write(rsSize,0,rsSize.Length);


            for (var x = 0; x < numBlocks; x++)
            {
                int bytesToRead = (int)((msIn.Position + 256 < msIn.Length ? 256 : msIn.Length - msIn.Position));
                byte[] data = new byte[bytesToRead];
                msIn.Read(data,0,bytesToRead);

                byte[] rsData = makeCodeWords(data, data.Length);

                msOut.Write(rsData,0,rsData.Length);
            }
            

            msOut.Flush();
            return msOut.ToArray();

        }

        private static byte[] makeCodeWords(byte[] dataBytes, int numEcBytesInBlock)
        {
            int numDataBytes = dataBytes.Length;
            int[] toEncode = new int[numDataBytes + numEcBytesInBlock];
            for (int i = 0; i < numDataBytes; i++)
            {
                toEncode[i] = dataBytes[i] & 0xFF;

            }
            new ReedSolomonEncoder(GenericGF.QR_CODE_FIELD_256).encode(toEncode, numEcBytesInBlock);

            byte[] byt = new byte[toEncode.Length];

            for (var x = 0; x < byt.Length; x++)
            {
                byt[x] = (byte)(toEncode[x] & 0xFF);
            }

            return byt;
        }


        private static byte[] correctErrors(byte[] codewordBytes, int numDataCodewords)
        {
            ReedSolomonDecoder rsDecoder = new ReedSolomonDecoder(GenericGF.QR_CODE_FIELD_256);

            int numCodewords = codewordBytes.Length;
            // First read into an array of ints
            int[] codewordsInts = new int[numCodewords];
            for (int i = 0; i < numCodewords; i++)
            {
                codewordsInts[i] = codewordBytes[i] & 0xFF;
            }
            int numECCodewords = codewordBytes.Length - numDataCodewords;

            if (!rsDecoder.decode(codewordsInts, numECCodewords))
                return new byte[] { 0 };

            // Copy back into array of bytes -- only need to worry about the bytes that were data
            // We don't care about errors in the error-correction codewords
            byte[] fixedData = new byte[numDataCodewords];

            for (int i = 0; i < numDataCodewords; i++)
            {
                fixedData[i] = (byte)codewordsInts[i];
            }



            return fixedData;
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


        public static T ExtractWatermark<T>(PNGFile file, string password)
        {
            Watermark m = ExtractWatermark(file, password);
            T converted = default(T);

            try
            {
                converted = (T)Convert.ChangeType(m, typeof(T));
            }
            catch (Exception ex) { }

            return converted;

        }

        /// <summary>
        /// Extracts a stored watermark from a PNGFile.
        /// </summary>
        /// <param name="file">PNGFile that contains the watermark.</param>
        /// <param name="mark">An empty watermark that will be populated.</param>
        /// <param name="password">Password that was used to embed the watermark.</param>
        /// <returns></returns>
        public static Watermark ExtractWatermark(PNGFile file, string password)
        {
            Rfc2898DeriveBytes bytes = new Rfc2898DeriveBytes(password, new byte[] { 112, 52, 63, 42, 180, 121, 53, 27 }, 1000);

            PNGScrambler scrambler = new PNGScrambler(file, bytes.GetBytes(16), bytes.GetBytes(8));
            byte[] data = null;
            byte markType;
            if (ReedSolomonProtection == false)
            {
                byte[] type = ReadBytes(file, scrambler, 1);
                markType= type[0];

                if (markType > 9) { return null; }
                byte[] dword = ReadBytes(file, scrambler, 4, 1);
                int length = BitConverter.ToInt32(dword, 0);

                try
                {
                    data = ReadBytes(file, scrambler, length, 5);
                }
                catch (Exception e) { return null; }
            }
            else
            {
                byte[] rsType = ReadBytes(file, scrambler, 3, 0);
                byte[] type = correctErrors(rsType, 1);
                markType = type[0];
                if ((markType & 0x80) != 0x80) { return null; }
                markType ^= 0x80;

                byte[] rsLength = ReadBytes(file, scrambler, 12, 3);
                byte[] length = correctErrors(rsLength, 4);
                int markLength = BitConverter.ToInt32(length,0);

                int position = 0;
                int numBlocks = (markLength / 256) + (markLength % 256 > 0 ? 1 : 0);

                MemoryStream msOut = new MemoryStream();

                while (numBlocks > 0)
                {
                    int bytesInBlock = (markLength - (position / 2) < 256 ? markLength - (position / 2) : 256);

                    bytesInBlock *= 2;

                    byte[] codeBytes = ReadBytes(file, scrambler, bytesInBlock, position + 15);

                    byte[] fixedData = correctErrors(codeBytes, bytesInBlock / 2);

                    msOut.Write(fixedData, 0, fixedData.Length);
                    numBlocks--;
                    position += bytesInBlock;

                    
                }

                data = msOut.ToArray();

            }

            Watermark mark = null;

            switch(markType)
            {
                case 1:
                    mark = TextWatermark.LoadFromBytes(data);
                    break;
                case 2:
                    mark = FileWatermark.LoadFromBytes(data);
                    break;
                case 3:
                    mark = BinaryWatermark.LoadFromBytes(data);
                    break;
                case 4:
                    mark = CompositeWatermark.LoadFromBytes(data);
                    break;
                case 9:
                    mark = EncryptedWatermark.LoadFromBytes(data);
                    break;
            }

            return mark;

        }
    }
}
