using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace PngWatermarker.Watermarks
{
    /// <summary>
    /// A watermark representing a file.
    /// </summary>
    public class FileWatermark : Watermark
    {
        public const int TYPE = 02;

        public byte[] fileData;
        public string extension;

        /// <summary>
        /// Constructor for a file based watermark.
        /// </summary>
        /// <param name="file">The file to load into this watermark.</param>
        public FileWatermark(String file)
        {
            if (File.Exists(file))
            {
                fileData = File.ReadAllBytes(file);
                extension = new FileInfo(file).Extension;
            }
        }

        internal FileWatermark(byte[] data, string extension)
        {
            this.fileData = data;
            this.extension = extension;
        }

        internal static FileWatermark LoadFromBytes(byte[] data)
        {
            int extLength = BitConverter.ToInt32(data, 0);
            string extension = System.Text.Encoding.UTF8.GetString(data, 4, extLength);

            int fileLength = BitConverter.ToInt32(data, 4 + extLength);
            byte[] fileData = new byte[fileLength];
            Array.Copy(data, 4 + extLength + 4, fileData, 0, fileLength);

            return new FileWatermark(fileData, extension);
        }
        internal override byte[] GetBytes()
        {
            MemoryStream ms = new MemoryStream();
            ms.WriteByte(TYPE);

            byte[] extBytes = System.Text.Encoding.UTF8.GetBytes(extension);
            byte[] extLength = BitConverter.GetBytes(extBytes.Length);
            

            byte[] fileLength = BitConverter.GetBytes(fileData.Length);

            int totalLength = extLength.Length + extBytes.Length + fileLength.Length + fileData.Length;
            byte[] totalLengthBytes = BitConverter.GetBytes(totalLength); 

            ms.Write(totalLengthBytes, 0, totalLengthBytes.Length);

            ms.Write(extLength, 0, extLength.Length);
            ms.Write(extBytes, 0, extBytes.Length);
            
            ms.Write(fileLength, 0, fileLength.Length);
            ms.Write(fileData, 0, fileData.Length);

            return ms.ToArray();
        }

        public override void Save(string output)
        {
            if (File.Exists(output))
            {
                File.Delete(output);
            }

            File.WriteAllBytes(output + extension, fileData);

        }

        internal override byte GetMarkType()
        {
            return TYPE;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("FileWatermark: ");
            sb.AppendLine(String.Format("\tExtension: {0}", extension));
            sb.AppendLine(String.Format("\tSize: {0}", fileData.Length));
            sb.Append("\tData: ");
            if (fileData.Length <= 20)
            {
                sb.AppendLine(BitConverter.ToString(fileData));
            }
            else
            {
                sb.AppendLine("Too Long to Display");
            }

            return sb.ToString();
        }
    }
}
