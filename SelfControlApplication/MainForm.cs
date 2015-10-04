using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Globalization;

namespace SelfControlApplication
{
    public partial class MainForm : Form
    {
        private string path = Path.GetPathRoot(Environment.SystemDirectory);
        private string temp = Path.GetTempPath();
        private string appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private string ShellCmdLocation = null;
        private string system32location = null;
        private string hostslocation = null;
        private string baklocation = null;
        Timer timer = new Timer();
        Timer timer1 = new Timer();
        private int timeLeft = 0;
        private int timerValue = 0;
        private DirectoryInfo dir = Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\SelfControlApp"); // создаем директорию приложения в AppData
        private string blockListPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\SelfControlApp\blocklist.rtf"; // файл с сохраненным списком блокируемых сайтов
        private RichTextBox rtbListP = new RichTextBox();
        List<string> phostdomains = new List<string>();

        public MainForm()
        {
            if (File.Exists(path + @"Windows\Sysnative\cmd.exe"))
            {
                ShellCmdLocation = path + @"Windows\Sysnative\cmd.exe";
                system32location = path + @"Windows\System32\";
                hostslocation = system32location + @"drivers\etc\hosts";
                baklocation = temp + @"hosts.bak";
            }
            else
            {
                ShellCmdLocation = path + @"Windows\System32\cmd.exe";
                system32location = path + @"Windows\System32\";
                hostslocation = system32location + @"drivers\etc\hosts";
                baklocation = temp + @"hosts.bak";
            }
            System.Threading.Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(Properties.Settings.Default.Language);
            System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(Properties.Settings.Default.Language);
            InitializeComponent();
        }

        
        private void button1_Click(object sender, EventArgs e)
        {
            BlockList blocklist = new BlockList();
            blocklist.ShowDialog();
            this.Show();
        }

        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void помощьToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            MessageBox.Show(LangStrings.HelpMessage, LangStrings.HelpTitle);
        }

        private void оПрограммеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            About about = new About();
            about.ShowDialog();
            this.Show();
        }

        private string ProcStartargs(string name, string args) // процесс для перезаписи hosts через консоль
        {
            try
            {
                var proc = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = name,
                        Arguments = args,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardInput = true,
                        CreateNoWindow = true,
                        StandardOutputEncoding = Encoding.UTF8,
                    }
                };
                proc.Start();
                string line = null;
                while (!proc.StandardOutput.EndOfStream)
                {
                    line += "\n" + proc.StandardOutput.ReadLine();
                }
                return line;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }


        private void DeleteFile(string filepath) 
        {
            ProcStartargs(ShellCmdLocation, "/c del /F /Q " + filepath);
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            timeLeft = tbarTime.Value * 15 * 60;
            timerValue = tbarTime.Value * 15 * 60;
            rtbListP.LoadFile(blockListPath);
            var hostsdomains = rtbListP.Lines;
            IdnMapping idn = new IdnMapping();
            for (int i = 0; i < hostsdomains.Length; i++) //все не ascii домены в punycode
            {
                if (!string.IsNullOrEmpty(hostsdomains[i]))
                {
                    string punycode = idn.GetAscii(hostsdomains[i]);
                    phostdomains.Add(punycode);
                }
            }
            File.Copy(hostslocation, baklocation, true);
            timer = new Timer();
            timer1 = new Timer();
            timer.Interval = timerValue * 1000; // 15 * 60 * 1000; // 15 минут.
            timer.Tick += timer_Tick;
            timer.Enabled = true;
            timer1.Interval = 1000; // Ежесекундный таймер.
            timer1.Tick += timer_Tick1;
            timer1.Enabled = true;
            lblTime.ForeColor = Color.Red;
            btnStart.Enabled = false;
            btnBlock.Enabled = false;
            tbarTime.Enabled = false;
            var hostsarray = phostdomains.ToArray();
            MessageBox.Show(LangStrings.WarningMessage, LangStrings.WarningTitle);

            if (true)
            {
                try
                {
                    string hosts = null;
                    if (File.Exists(hostslocation))
                    {
                        hosts = File.ReadAllText(hostslocation, Encoding.UTF8);
                        File.SetAttributes(hostslocation, FileAttributes.Normal);
                        DeleteFile(hostslocation);
                    }
                    File.Create(hostslocation).Close();
                    File.WriteAllText(hostslocation, hosts + "\r\n#!!!DON'T MODIFY TEXT BELOW!!!\r\n#!!!SELF CONTROL APP BLOCKLIST!!!\r\n", Encoding.UTF8);
                    for (int i = 0; i < hostsarray.Length; i++)
                    {
                        if (hosts.IndexOf(hostsarray[i]) == -1)
                        {
                            ProcStartargs(ShellCmdLocation,
                                "/c echo " + "0.0.0.0 " + hostsarray[i] + " >> \"" + hostslocation +
                                "\"");
                            ProcStartargs(ShellCmdLocation,
                                "/c echo " + "0.0.0.0 www." + hostsarray[i] + " >> \"" + hostslocation +
                                "\"");
                        }
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("Ошибка добавления в hosts.\nЗапустите программу от имени Администратора!");
                }
            }
        }

        void timer_Tick(object sender, EventArgs e)
        {
            timer.Enabled = false; // Останавливаем таймер.
            btnStart.Enabled = true;
            btnBlock.Enabled = true;
            tbarTime.Enabled = true;

            // Восстанавливаемся из резервной копии.
            File.Copy(baklocation, hostslocation, true);
            File.Delete(baklocation);
        }

        void timer_Tick1(object sender, EventArgs e)
        {
            if (timeLeft != 0)
            {
                timeLeft--;
                var s = TimeSpan.FromSeconds(timeLeft);
                lblTime.Text = s.ToString();
            }
            else
            {
                timer1.Enabled = false;
                lblTime.ForeColor = Color.Black;
            }
        }

        private void tbarTime_Scroll(object sender, EventArgs e)
        {
            int lValue = tbarTime.Value * 15;
            string hour = Convert.ToString(lValue / 60);
            string mins = Convert.ToString(lValue % 60);
            if (mins.Length < 2) mins = "0" + mins;
            string lbl = String.Format("0{0}:{1}:00", hour, mins);
            lblTime.Text = lbl;
        }

        private void niTray_Click(object sender, EventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
        }

        private void cmExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (FormWindowState.Minimized == WindowState) Hide();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
                WindowState = FormWindowState.Minimized;
            }
        }

        private void englishToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (System.Threading.Thread.CurrentThread.CurrentUICulture.Name == "ru-RU")
            {
                System.Threading.Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
                System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
                Properties.Settings.Default.Language = "en-US";
                Properties.Settings.Default.Save();
                Application.Restart();

            }
            else
            {
                System.Threading.Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("ru-RU");
                System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");
                Properties.Settings.Default.Language = "ru-RU";
                Properties.Settings.Default.Save();
                Application.Restart();
            }
        }
    }
}
