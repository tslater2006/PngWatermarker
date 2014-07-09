using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hjg.Pngcs;
using Hjg.Pngcs.Chunks;
namespace PngWatermarker
{
    public class PNGFile
    {
        public List<PNGPixel[]> lines = new List<PNGPixel[]>();
        public bool hasAlpha;
        public string originalFile;
        public readonly ImageInfo ImgInfo;

        public int EstimatedStorage
        {
            get
            {
                int pixels = lines.Count * lines[0].Length;
                //pixels -= 11; // can't count the pixels needed for salt storage

                int totalBits = pixels * 6;
                int totalBytes = totalBits / 8;

                return totalBytes;
            }
        }

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
    public class PNGPixel
    {
        public byte Red;
        public byte Green;
        public byte Blue;
        public byte Alpha;
        
    }

}
