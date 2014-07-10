using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace PngWatermarker.Watermarks
{
    public class CompositeWatermark : Watermark
    {
        public const int TYPE = 04;
        private List<Watermark> watermarks = new List<Watermark>();

        public CompositeWatermark()
        {

        }
        public override byte GetMarkType()
        {
            return TYPE;
        }
        public override byte[] GetBytes()
        {
            MemoryStream ms = new MemoryStream();
            ms.WriteByte(TYPE);

            List<byte[]> marksB = new List<byte[]>();
            int totalMarksSize = 0;
            foreach(Watermark m in watermarks)
            {
                marksB.Add(m.GetBytes());
                totalMarksSize+= marksB[marksB.Count -1].Length;
            }

            ms.Write(BitConverter.GetBytes(totalMarksSize), 0, 4);

            foreach(byte[] data in marksB)
            {
                ms.Write(data, 0, data.Length);
            }
            return ms.ToArray();
        }

        public override bool LoadFromBytes(byte[] data)
        {
            MemoryStream ms = new MemoryStream(data);
            byte[] type = new byte[1];
            byte[] dword = new byte[4];
            byte[] markData;
            while(ms.Position < ms.Length)
            {
                ms.Read(type, 0, 1);
                ms.Read(dword, 0, 4);

                markData = new byte[BitConverter.ToInt32(dword, 0)];
                ms.Read(markData, 0, markData.Length);
                Watermark mark = new BinaryWatermark();
                switch (type[0])
                {
                    case 1:
                        mark = new TextWatermark();
                        break;
                    case 2:
                        mark = new FileWatermark();
                        break;
                    case 3:
                        mark = new BinaryWatermark();
                        break;
                    case 4:
                        mark = new CompositeWatermark();
                        break;
                }
                mark.LoadFromBytes(markData);
                
                watermarks.Add(mark);
            }
            return true;

        }
        public int GetWatermarkCount()
        {
            return watermarks.Count;
        }

        public Watermark[] GetWatermarks()
        {
            return watermarks.ToArray<Watermark>();
        }

        public void AddWatermark(Watermark mark)
        {
            if (mark.GetMarkType() != 9)
            {
                watermarks.Add(mark);
            }
        }

        public void RemoveWatermark(Watermark mark)
        {
            watermarks.Remove(mark);
        }

    }
}
