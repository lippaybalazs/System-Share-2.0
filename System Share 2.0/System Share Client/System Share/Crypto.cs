using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace System_Share
{
    class Crypto
    {
        public static string rsaPublic;
        public static string rsaPrivate;
        public static byte[] IV = null;
        public static byte[] Key = null;

        /// <summary>
        /// Generates and returns the input vector
        /// </summary>
        public static byte[] GetVector()
        {
            Random rng = new Random();
            string seedValue = rng.Next(0, int.MaxValue).ToString() + rng.Next(0, int.MaxValue).ToString() + rng.Next(0, int.MaxValue).ToString() + rng.Next(0, int.MaxValue).ToString();
            byte[] seedBytes = Encoding.UTF8.GetBytes(seedValue);
            MD5 hash = MD5.Create();
            byte[] iv = hash.ComputeHash(seedBytes);
            IV = iv;
            return iv;
        }

        /// <summary>
        /// Generates and returns the key
        /// </summary>
        public static byte[] GetKey()
        {
            Random rng = new Random();
            string seedValue = rng.Next(0, int.MaxValue).ToString() + rng.Next(0, int.MaxValue).ToString() + rng.Next(0, int.MaxValue).ToString() + rng.Next(0, int.MaxValue).ToString();
            byte[] seedBytes = Encoding.UTF8.GetBytes(seedValue);
            MD5 hash = MD5.Create();
            byte[] key = hash.ComputeHash(seedBytes);
            Key = key;
            return key;
        }

        /// <summary>
        /// Encrypts the given string
        /// </summary>
        public static byte[] Encrypt(string cmd, bool work)
        {
            if (work)
            {
                byte[] cryptoBytes;

                using (Aes crypt = Aes.Create())
                {
                    crypt.Key = Key;
                    crypt.IV = IV;

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
        public static string Decrypt(byte[] cipher, bool work)
        {
            if (work)
            {
                string cmd = null;

                using (Aes crypt = Aes.Create())
                {
                    crypt.Key = Key;
                    crypt.IV = IV;

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



        
        /// <summary>
        /// Generates a private key, and returns a public key
        /// </summary>
        public static string RSAGenerate()
        {
            var rsa = new RSACryptoServiceProvider();
            rsaPrivate = rsa.ToXmlString(true);
            return rsa.ToXmlString(false);
        }

        /// <summary>
        /// Decrypts the string
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
        /// Encrypts the string
        /// </summary>
        public static string Encrypt(string data, string publicKey)
        {
            var encoder = new UnicodeEncoding();
            var rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(publicKey);
            var dataToEncrypt = encoder.GetBytes(data);
            var encryptedByteArray = rsa.Encrypt(dataToEncrypt, false).ToArray();
            var length = encryptedByteArray.Count();
            var item = 0;
            var sb = new StringBuilder();
            foreach (var x in encryptedByteArray)
            {
                item++;
                sb.Append(x);

                if (item < length)
                    sb.Append(",");
            }

            return sb.ToString();
        }
    }
}
