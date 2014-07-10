using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hjg.Pngcs;
using Hjg.Pngcs.Chunks;
namespace PngWatermarker
{
    /// <summary>
    /// Loads a PNG file to memory for use with Watermarker
    /// </summary>
    public class PNGFile
    {
        /// <summary>
        /// The lines of a PNG file, each line is an array of PNGPixel
        /// </summary>
        public List<PNGPixel[]> lines = new List<PNGPixel[]>();

        /// <summary>
        /// Wether or not the image has an alpha channel.
        /// </summary>
        public bool hasAlpha;

        /// <summary>
        /// Stores the path to the original file.
        /// </summary>
        public string originalFile;

        /// <summary>
        /// Basic PNG Image Inforamtion.
        /// </summary>
        public readonly ImageInfo ImgInfo;

        /// <summary>
        /// Total storage capacity of the PNG file.
        /// </summary>
        public int EstimatedStorage
        {
            get
            {
                int pixels = lines.Count * lines[0].Length;
                //pixels -= 11; // can't count the pixels needed for salt storage

                int totalBits = pixels * 6;
                int totalBytes = totalBits / 8;

                return totalBytes - 5; // subtract 5 bytes for watermark header
            }
        }
        
        /// <summary>
        /// Main constructor for PNGFile.
        /// </summary>
        /// <param name="path">Path to the PNG file to be loaded.</param>
        public PNGFile(string path)
        {
            originalFile = path;
            var reader = FileHelper.CreatePngReader(path);
            if (reader.ImgInfo.BitDepth != 8 || reader.ImgInfo.Channels < 3)
            {
                throw new ArgumentException("The PNG file must be an 24/32 bit PNG file, 8bit channels and RGB/A");
            }
            this.ImgInfo = reader.ImgInfo;
            reader.ShouldCloseStream = true;
            hasAlpha = reader.ImgInfo.Alpha;
            for (var x = 0; x < reader.ImgInfo.Rows; x++)
            {
                PNGPixel[] line = new PNGPixel[reader.ImgInfo.Cols];

                byte[] scanLine = reader.ReadRowByte(x).ScanlineB;

                int bytesPerPixel = hasAlpha ? 4 : 3;
                int byteCount = 0;
                int pixelCount = 0;
                PNGPixel curPixel = new PNGPixel();
                for (var y = 0; y < scanLine.Length; y++)
                {
                    byteCount++;

                    if (byteCount > bytesPerPixel || y == (scanLine.Length -1))
                    {
                        line[pixelCount++] = curPixel;

                        byteCount = 1;
                        curPixel = new PNGPixel();
                    }

                    if (byteCount == 1)
                    {
                        curPixel.Red = scanLine[y];
                    } else if (byteCount == 2)
                    {
                        curPixel.Green = scanLine[y];
                    } else if (byteCount == 3)
                    {
                        curPixel.Blue = scanLine[y];
                    } else if (byteCount == 4)
                    {
                        curPixel.Alpha = scanLine[y];
                    }

                }
                lines.Add(line);
            }
            reader.End();
            reader = null;
        }

        /// <summary>
        /// Saves the PNGFile to disk.
        /// </summary>
        /// <param name="path">Output path for the PNGFile.</param>
        public void SaveAs(string path)
        {
            PngReader reader = FileHelper.CreatePngReader(originalFile);
            PngWriter writer = FileHelper.CreatePngWriter(path, reader.ImgInfo, true);

            writer.CopyChunksFirst(reader, ChunkCopyBehaviour.COPY_ALL_SAFE);

            for (var x = 0; x < reader.ImgInfo.Rows; x++ )
            {
                ImageLine line = reader.ReadRowByte(x);
                for (var y = 0; y < line.ScanlineB.Length; y+= reader.ImgInfo.BytesPixel)
                {
                    line.ScanlineB[y] = lines[x][y / reader.ImgInfo.BytesPixel].Red;
                    line.ScanlineB[y+1] = lines[x][(y+1) / reader.ImgInfo.BytesPixel].Green;
                    line.ScanlineB[y+2] = lines[x][(y +2) / reader.ImgInfo.BytesPixel].Blue;
                    if (reader.ImgInfo.Alpha)
                    {
                        line.ScanlineB[y + 3] = lines[x][(y + 3) / reader.ImgInfo.BytesPixel].Alpha;
                    }
                }
                writer.WriteRow(line, x);
            }
            writer.CopyChunksLast(reader, ChunkCopyBehaviour.COPY_ALL_SAFE);
            writer.End();
            reader.End();
        }
    }

    /// <summary>
    /// Basic representation of a Pixel
    /// </summary>
    /// <remarks>
    /// Must be a Class and not a Struct so that we can pass by reference and not copies.
    /// This is required for the PNGScrambler to work properly.
    /// </remarks>
    public class PNGPixel
    {
        public byte Red;
        public byte Green;
        public byte Blue;
        public byte Alpha;
        
    }

}
