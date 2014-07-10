using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;
namespace PngWatermarker.Watermarks
{
    public class EncryptedWatermark : Watermark
    {
        public const int TYPE = 09;

        public Watermark DecryptedMark;

        protected byte[] cryptedData;
        protected byte[] salt;
        protected byte[] key;

        private Rfc2898DeriveBytes bytes;
        private string password;
        private SymmetricAlgorithm algo;

        public EncryptedWatermark(Watermark mark, SymmetricAlgorithm algo, string password)
        {
            if (mark.GetMarkType() == this.GetMarkType())
            {
                throw new ArgumentException("You cannot next encrypted watermarks!");
            }
            // get the base marks bytes
            byte[] markBytes = mark.GetBytes();

            bytes = new Rfc2898DeriveBytes(password, 8);
            this.salt = bytes.Salt;

            key = bytes.GetBytes(algo.KeySize / 8);

            if (algo.IV == null)
            {
                algo.IV = bytes.GetBytes(algo.BlockSize);
            }
            algo.Key = key;
            byte[] iv = algo.IV;

            byte[] cryptData = Encrypt(markBytes, algo);

            MemoryStream ms = new MemoryStream();

            byte[] ivLength = BitConverter.GetBytes(iv.Length);
            ms.Write(ivLength, 0, ivLength.Length);

            ms.Write(iv, 0, iv.Length);

            byte[] dataLength = BitConverter.GetBytes(cryptData.Length);
            ms.Write(dataLength, 0, dataLength.Length);

            ms.Write(cryptData, 0, cryptData.Length);

            cryptedData = ms.ToArray();
        }

        public EncryptedWatermark(SymmetricAlgorithm algo, string password)
        {
            this.password = password;
            this.algo = algo;
        }

        internal override byte GetMarkType()
        {
            return TYPE;
        }
        internal override bool LoadFromBytes(byte[] data)
        {
            int saltLength = BitConverter.ToInt32(data, 0);
            byte[] salt = new byte[saltLength];
            Array.Copy(data, 4, salt, 0, saltLength);
            bytes = new Rfc2898DeriveBytes(password, salt);

            this.salt = salt;
            this.key = bytes.GetBytes(this.algo.KeySize/8);
            algo.Key = key;
            int cryptedChunkLength = BitConverter.ToInt32(data, 4 + saltLength);
            byte[] cryptedChunk = new byte[cryptedChunkLength];
            Array.Copy(data, 4 + saltLength + 4 , cryptedChunk, 0, cryptedChunkLength);

            int ivLength = BitConverter.ToInt32(cryptedChunk, 0);
            byte[] iv = new byte[ivLength];

            Array.Copy(cryptedChunk, 4, iv, 0, ivLength);
            algo.IV = iv;

            int cryptDataLength = BitConverter.ToInt32(cryptedChunk, 4 + ivLength);

            byte[] cryptedData = new byte[cryptDataLength];

            Array.Copy(cryptedChunk, 4 + ivLength + 4, cryptedData, 0, cryptDataLength);

            byte[] decrypted = Decrypt(cryptedData, algo);

            byte markType = decrypted[0];
            int markDataLength = BitConverter.ToInt32(decrypted,1);
            
            byte[] markData = new byte[markDataLength];

            Array.Copy(decrypted, 5, markData, 0, markDataLength);

            switch (markType)
            {
                case 1:
                    DecryptedMark = new TextWatermark();
                    break;
                case 2:
                    DecryptedMark = new FileWatermark();
                    break;
                case 3:
                    DecryptedMark = new BinaryWatermark();
                    break;
                case 4:
                    DecryptedMark = new CompositeWatermark();
                    break;
            }

            DecryptedMark.LoadFromBytes(markData);

            return true;
        }

        public override byte[] GetBytes()
        {
            MemoryStream ms = new MemoryStream();

            ms.WriteByte(TYPE);

            byte[] saltLength = BitConverter.GetBytes(salt.Length);
            byte[] cryptLength = BitConverter.GetBytes(cryptedData.Length);

            byte[] totalLength = BitConverter.GetBytes(saltLength.Length + salt.Length + cryptLength.Length + cryptedData.Length);

            ms.Write(totalLength, 0, totalLength.Length);

            ms.Write(saltLength, 0, saltLength.Length);

            ms.Write(salt, 0, salt.Length);

            ms.Write(cryptLength, 0, cryptLength.Length);

            ms.Write(cryptedData, 0, cryptedData.Length);

            return ms.ToArray();
        }


        private static Byte[] Encrypt(Byte[] input, SymmetricAlgorithm crypto)
        {
            return Transform(crypto.CreateEncryptor(), input, crypto.BlockSize);
        }

        private static Byte[] Decrypt(Byte[] input, SymmetricAlgorithm crypto)
        {
            return Transform(crypto.CreateDecryptor(), input, crypto.BlockSize);
        }

        private static Byte[] Transform(ICryptoTransform cryptoTransform, Byte[] input, Int32 blockSize)
        {
            if (input.Length > blockSize)
            {
                Byte[] ret1 = new Byte[((input.Length - 1) / blockSize) * blockSize];

                Int32 inputPos = 0;
                Int32 ret1Length = 0;
                for (inputPos = 0; inputPos < input.Length - blockSize; inputPos += blockSize)
                {
                    ret1Length += cryptoTransform.TransformBlock(input, inputPos, blockSize, ret1, ret1Length);
                }

                Byte[] ret2 = cryptoTransform.TransformFinalBlock(input, inputPos, input.Length - inputPos);

                Byte[] ret = new Byte[ret1Length + ret2.Length];
                Array.Copy(ret1, 0, ret, 0, ret1Length);
                Array.Copy(ret2, 0, ret, ret1Length, ret2.Length);
                return ret;
            }
            else
            {
                return cryptoTransform.TransformFinalBlock(input, 0, input.Length);
            }

        }
    }
}
