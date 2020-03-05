using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace System_Share
{
    class Crypto
    {
        public static string rsaPrivate;
        public static string rsaPublic;

        /// <summary>
        /// Generates keys
        /// </summary>
        public static void RSAGenerate()
        {
            var rsa = new RSACryptoServiceProvider();
            rsaPrivate = rsa.ToXmlString(true);
            rsaPublic =  rsa.ToXmlString(false);
        }

        /// <summary>
        /// Decripts the given string
        /// </summary>
        public static string Decrypt(string data, string privateKey)
        {
            var encoder = new UnicodeEncoding();
            var rsa = new RSACryptoServiceProvider();
            var dataArray = data.Split(new char[] { ',' });
            byte[] dataByte = new byte[dataArray.Length];
            for (int i = 0; i < dataArray.Length; i++)
            {
                dataByte[i] = Convert.ToByte(dataArray[i]);
            }

            rsa.FromXmlString(privateKey);
            var decryptedByte = rsa.Decrypt(dataByte, false);
            return encoder.GetString(decryptedByte);
        }

        /// <summary>
        /// Encrypts the given string
        /// </summary>
        public static byte[] Encrypt(string cmd, byte[] key, byte[] iv)
        {
            if (key != null)
            {
                byte[] cryptoBytes;

                using (Aes crypt = Aes.Create())
                {
                    crypt.Key = key;
                    crypt.IV = iv;

                    ICryptoTransform encryptor = crypt.CreateEncryptor(crypt.Key, crypt.IV);

                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                        {
                            using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
                            {
                                streamWriter.Write(cmd);
                            }
                            cryptoBytes = memoryStream.ToArray();
                        }
                    }
                }
                return cryptoBytes;
            }
            else
            {
                return Encoding.UTF8.GetBytes(cmd);
            }
        }

        /// <summary>
        /// Decrypts the given array of bytes
        /// </summary>
        public static string Decrypt(byte[] cipher, byte[] key, byte[] iv)
        {
            if (key != null)
            {
                string cmd = null;

                using (Aes crypt = Aes.Create())
                {
                    crypt.Key = key;
                    crypt.IV = key;

                    ICryptoTransform decryptor = crypt.CreateDecryptor(crypt.Key, crypt.IV);

                    using (MemoryStream memoryStream = new MemoryStream(cipher))
                    {
                        using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader streamWriter = new StreamReader(cryptoStream))
                            {
                                cmd = streamWriter.ReadToEnd();
                            }
                        }
                    }
                }

                return cmd;
            }
            else
            {
                return Encoding.UTF8.GetString(cipher);
            }
        }
    }
}
