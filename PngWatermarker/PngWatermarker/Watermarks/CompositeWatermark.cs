using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace PngWatermarker.Watermarks
{
    /// <summary>
    /// A watermark that contains other watermarks.
    /// </summary>
    public class CompositeWatermark : Watermark
    {
        public const int TYPE = 04;
        internal List<Watermark> watermarks = new List<Watermark>();
        
        /// <summary>
        /// gets the list of watermarks contained in this CompositeWatermark.
        /// </summary>
        public List<Watermark> Watermarks { get { return watermarks; } }

        internal override byte GetMarkType()
        {
            return TYPE;
        }
        internal override byte[] GetBytes()
        {
            MemoryStream ms = new MemoryStream();
            ms.WriteByte(TYPE);

            List<byte[]> marksB = new List<byte[]>();
            int totalMarksSize = 0;
            foreach(Watermark m in watermarks)
            {
                if (m.GetMarkType() != 9) {
                    marksB.Add(m.GetBytes());
                    totalMarksSize+= marksB[marksB.Count -1].Length;
                }
            }

            ms.Write(BitConverter.GetBytes(totalMarksSize), 0, 4);

            foreach(byte[] data in marksB)
            {
                ms.Write(data, 0, data.Length);
            }
            return ms.ToArray();
        }

        internal static CompositeWatermark LoadFromBytes(byte[] data)
        {
            CompositeWatermark comp = new CompositeWatermark();

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
                Watermark mark = null;
                switch (type[0])
                {
                    case 1:
                        mark = TextWatermark.LoadFromBytes(markData);
                        break;
                    case 2:
                        mark = FileWatermark.LoadFromBytes(markData);
                        break;
                    case 3:
                        mark = BinaryWatermark.LoadFromBytes(markData);
                        break;
                    case 4:
                        mark = CompositeWatermark.LoadFromBytes(markData);
                        break;
                }
                
                comp.watermarks.Add(mark);
            }
            return comp;

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
