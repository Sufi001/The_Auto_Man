using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Inventory.Encryption
{
        public class RijndaelManagedEncryption
        {
            //internal const string Inputkey = "651A20CD-0322-4CF0-A2E8-671F1B6B9EA17";
            internal const string Inputkey = "651A20CD-0322-4CF0-A2E8-671F1B6B9EA17-32877AGD-3783625-A98H76";
            public static string EncryptRijndael(string text)
            {
                if (string.IsNullOrEmpty(text))
                    throw new ArgumentNullException("text");

            var salt = "SOFTVALLEY-TOWNSHIP"; 
            var aesAlg = NewRijndaelManaged(salt);

                var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
                var msEncrypt = new MemoryStream();
                using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                using (var swEncrypt = new StreamWriter(csEncrypt))
                {
                    swEncrypt.Write(text);
                }

                return Convert.ToBase64String(msEncrypt.ToArray());
            }
            public static bool IsBase64String(string base64String)
            {
                base64String = base64String.Trim();
                return (base64String.Length % 4 == 0) &&
                       Regex.IsMatch(base64String, @"^[a-zA-Z0-9\+/]*={0,3}$", RegexOptions.None);

            }
            public static string DecryptRijndael(string cipherText)
            {
                if (string.IsNullOrEmpty(cipherText))
                    throw new ArgumentNullException("cipherText");

                if (!IsBase64String(cipherText))
                    throw new Exception("The cipherText input parameter is not base64 encoded");

                var salt = "SOFTVALLEY-TOWNSHIP";
                string text;

                var aesAlg = NewRijndaelManaged(salt);
                var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                var cipher = Convert.FromBase64String(cipherText);

                using (var msDecrypt = new MemoryStream(cipher))
                {
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (var srDecrypt = new StreamReader(csDecrypt))
                        {
                            text = srDecrypt.ReadToEnd();
                        }
                    }
                }
                return text;
            }
            private static RijndaelManaged NewRijndaelManaged(string salt)
            {
                if (salt == null) throw new ArgumentNullException("salt");
                var saltBytes = Encoding.ASCII.GetBytes(salt);
                var key = new Rfc2898DeriveBytes(Inputkey, saltBytes);

                var aesAlg = new RijndaelManaged();
                aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);
                aesAlg.IV = key.GetBytes(aesAlg.BlockSize / 8);

                return aesAlg;
            }
        }
}