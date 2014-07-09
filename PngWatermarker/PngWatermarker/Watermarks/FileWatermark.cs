﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace PngWatermarker.Watermarks
{
    public class FileWatermark : Watermark
    {
        public const int TYPE = 02;

        public byte[] fileData;
        public string extension;

        public FileWatermark(String file)
        {
            if (File.Exists(file))
            {
                fileData = File.ReadAllBytes(file);
                extension = new FileInfo(file).Extension;
            }
        }


        public override byte[] GetBytes()
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

    }
}
