using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;
namespace PngWatermarker.Watermarks
{
    /// <summary>
    /// Represents an encrypted watermark.
    /// </summary>
    public class EncryptedWatermark : Watermark
    {
        /// <summary>
        /// Crypto Algorithm used for Encrypted Watermarks
        /// </summary>
        public static SymmetricAlgorithm Algorithm;
        public const int TYPE = 09;

        public Watermark DecryptedMark;

        protected byte[] cryptedData = null;
        protected byte[] salt;
        protected byte[] key;

        private Rfc2898DeriveBytes bytes;

        /// <summary>
        /// Constructor for an encrypted watermark.
        /// </summary>
        /// <param name="mark">The watermark to encrypt.</param>
        /// <param name="password">The password which is used to derive the encryption key.</param>
        public EncryptedWatermark(Watermark mark, string password)
        {
            /*if (mark.GetMarkType() == this.GetMarkType())
            {
                throw new ArgumentException("You cannot next encrypted watermarks!");
            }*/
            // get the base marks bytes
            byte[] markBytes = mark.GetBytes();

            bytes = new Rfc2898DeriveBytes(password, 8);
            this.salt = bytes.Salt;

            key = bytes.GetBytes(Algorithm.KeySize / 8);

            if (Algorithm.IV == null)
            {
                Algorithm.IV = bytes.GetBytes(Algorithm.BlockSize);
            }
            Algorithm.Key = key;
            byte[] iv = Algorithm.IV;

            byte[] cryptData = Encrypt(markBytes, Algorithm);

            MemoryStream ms = new MemoryStream();

            byte[] ivLength = BitConverter.GetBytes(iv.Length);
            ms.Write(ivLength, 0, ivLength.Length);

            ms.Write(iv, 0, iv.Length);

            byte[] dataLength = BitConverter.GetBytes(cryptData.Length);
            ms.Write(dataLength, 0, dataLength.Length);

            ms.Write(cryptData, 0, cryptData.Length);

            cryptedData = ms.ToArray();
        }

        internal EncryptedWatermark(byte[] cryptedData, byte[] salt)
        {
            this.cryptedData = cryptedData;
            this.salt = salt;

        }

        internal override byte GetMarkType()
        {
            return TYPE;
        }

        /// <summary>
        /// Method for decrypting an extracted watermark.
        /// </summary>
        /// <param name="password">The password that was used during encryption.</param>
        public void Decrypt(string password)
        {
            bytes = new Rfc2898DeriveBytes(password, salt);

            this.key = bytes.GetBytes(Algorithm.KeySize/8);
            Algorithm.Key = key;

            int ivLength = BitConverter.ToInt32(cryptedData, 0);
            byte[] iv = new byte[ivLength];

            Array.Copy(cryptedData, 4, iv, 0, ivLength);
            Algorithm.IV = iv;

            int cryptDataLength = BitConverter.ToInt32(cryptedData, 4 + ivLength);

            byte[] cryptedData2 = new byte[cryptDataLength];

            Array.Copy(cryptedData, 4 + ivLength + 4, cryptedData2, 0, cryptDataLength);

            byte[] decrypted = Decrypt(cryptedData2, Algorithm);

            byte markType = decrypted[0];
            int markDataLength = BitConverter.ToInt32(decrypted,1);
            
            byte[] markData = new byte[markDataLength];

            Array.Copy(decrypted, 5, markData, 0, markDataLength);

            switch (markType)
            {
                case 1:
                    DecryptedMark = TextWatermark.LoadFromBytes(markData);
                    break;
                case 2:
                    DecryptedMark = FileWatermark.LoadFromBytes(markData);
                    break;
                case 3:
                    DecryptedMark = BinaryWatermark.LoadFromBytes(markData);
                    break;
                case 4:
                    DecryptedMark = CompositeWatermark.LoadFromBytes(markData);
                    break;
                case 9:
                    DecryptedMark = EncryptedWatermark.LoadFromBytes(markData);
                    break;
            }
        }

        internal static EncryptedWatermark LoadFromBytes(byte[] data)
        {
            int saltLength = BitConverter.ToInt32(data, 0);
            byte[] salt = new byte[saltLength];
            Array.Copy(data, 4, salt, 0, saltLength);

            int cryptedChunkLength = BitConverter.ToInt32(data, 4 + saltLength);
            byte[] cryptedChunk = new byte[cryptedChunkLength];
            Array.Copy(data, 4 + saltLength + 4 , cryptedChunk, 0, cryptedChunkLength);

            EncryptedWatermark mark = new EncryptedWatermark(cryptedChunk, salt);


            return mark;
        }

        internal override byte[] GetBytes()
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
