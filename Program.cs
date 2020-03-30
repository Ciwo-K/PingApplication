using System;
using System.Text;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Drawing;
using System.IO;

namespace Pinger
{
    public class Worker
    {
        private string adress = "google.de"; //= "8.8.8.8";
        private Ping pingSender = new Ping();
        private PingOptions options = new PingOptions();
        private string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        private int timeout = 999;
        private int timeOfFlight;
        private List<int> lastXPings = new List<int>();
        private int greenRange;
        private int yellowRange;
        private int pingsPerMinute;
        private byte[] buffer;
        private int ignoreTimeouts;
        private int tempTimeouts;

        public int xDefinition { get; set; }

        public Worker(int xDefinition = 3, int greenRange = 50, int yellowRange = 130, int pingsPerMinute = 60, int ignoreTimeouts = 3)
        {
            options.DontFragment = true;
            this.xDefinition = xDefinition;
            this.greenRange = greenRange;
            this.yellowRange = yellowRange;
            this.pingsPerMinute = pingsPerMinute;
            this.ignoreTimeouts = ignoreTimeouts;
            buffer = Encoding.ASCII.GetBytes(data);
            SetAddress(adress);
        }

        public void SetAddress(string adress)
        {
            if (!Regex.IsMatch(adress, @"^([0-9]|\d{2}[0-9]|\d{3}[0-9]).([0-9]|\d{2}[0-9]|\d{3}[0-9]).([0-9]|\d{2}[0-9]|\d{3}[0-9]).([0-9]|\d{2}[0-9]|\d{3}[0-9])$")) //xxx.xxx.xxx.xxx
            {
                IPHostEntry hostInfo = Dns.GetHostEntry(adress);
                this.adress = hostInfo.AddressList.GetValue(0).ToString();
                return;
            }
            this.adress = adress;
        }
        public void SetRanges(string green, string yellow)
        {
            int temp = 0;
            if (int.TryParse(green, out temp))
            {
                greenRange = temp;
            }
            if (int.TryParse(yellow, out temp))
            {
                if (temp > greenRange)
                {
                    yellowRange = temp;
                }
                else
                {
                    yellowRange = greenRange;
                }
            }
        }
        public void SetPingsPerMinute(int pingsPerMinute)
        {
            if (pingsPerMinute <= 240 && pingsPerMinute > 0)
            {
                this.pingsPerMinute = pingsPerMinute;
            }
            else
            {
                this.pingsPerMinute = 240;
            }
        }

        public string GetAdress() { return adress; }
        public int GetGreenRange() { return greenRange; }
        public int GetYellowRange() { return yellowRange; }
        public int GetLastXAverage()
        {
            int temp = 0;
            foreach (int i in lastXPings)
            {
                temp += i;
            }
            if (lastXPings.Count > 0)
            {
                temp /= lastXPings.Count;
            }
            return temp;
        }
        public int GetPingsPerMinute() { return pingsPerMinute; }

        public int ActualPing()
        {
            try
            {
                timeOfFlight = Ping(adress, timeout, buffer, options, pingSender);
                lastXPings.Add(timeOfFlight);
                if (lastXPings.Count > xDefinition)
                {
                    lastXPings.RemoveAt(0);
                }
                return timeOfFlight;
            }
            catch (Exception e)
            {

            }
            if (lastXPings.Count > 0)
            {
                return lastXPings.ToArray()[lastXPings.Count - 1];

            }
            else
            {
                return 999;
            }
        }

        int Ping(string pingAdress, int timeout, byte[] buffer, PingOptions options, Ping pingSender)
        {
            PingReply reply = pingSender.Send(pingAdress, timeout, buffer, options);
            if (reply.Status == IPStatus.Success)
            {
                tempTimeouts = 0;
                return (int)reply.RoundtripTime;
            }
            else if (tempTimeouts < ignoreTimeouts)
            {
                tempTimeouts++;
                return lastXPings.ToArray()[lastXPings.Count - 1];
            }
            else return 999;
        }
    }

    class Program
    {
        private static NotifyIcon notico;
        private static Form1 myForm;
        static Thread thread;

        public static Worker PingWorker = new Worker();

        [STAThread]
        public static void Main()
        {
            ContextMenu cm;
            MenuItem miCurr;
            int iIndex = 0;

            // Kontextmenü erzeugen
            cm = new ContextMenu();

            //// Kontextmenüeinträge erzeugen
            //miCurr = new MenuItem();
            //miCurr.Index = iIndex++;
            //miCurr.Text = "&Aktion 1";           // Eigenen Text einsetzen
            //miCurr.Click += new System.EventHandler(Action1Click);
            //cm.MenuItems.Add(miCurr);

            // Kontextmenüeinträge erzeugen
            miCurr = new MenuItem();
            miCurr.Index = iIndex++;
            miCurr.Text = "&Exit";
            miCurr.Click += new System.EventHandler(ExitClick);
            cm.MenuItems.Add(miCurr);

            // NotifyIcon selbst erzeugen
            notico = new NotifyIcon();
            NotifyIcon good = new NotifyIcon();
            NotifyIcon bad = new NotifyIcon();
            NotifyIcon okey = new NotifyIcon();
            good.Icon = new Icon("data\\good.ico");
            okey.Icon = new Icon("data\\okey.ico");
            bad.Icon = new Icon("data\\bad.ico");

            Application.ThreadExit += OnClosing;

            notico.Icon = bad.Icon; // Eigenes Icon einsetzen
            notico.Text = "Ping Application";   // Eigenen Text einsetzen
            notico.Visible = true;
            notico.ContextMenu = cm;
            notico.DoubleClick += new EventHandler(NotifyIconDoubleClick);
            LoadSaveState(PingWorker);

            ////          Windows startup
            //using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run" , true))
            //{
            //    if (key.GetValue("PingLite") != null)
            //    {
            //        key.SetValue("PingLite", "\"" + Application.ExecutablePath + "\"");
            //    }
            //}

            thread = new Thread(() =>
            {
                int tempInt = 0;
                Icon tempIcon;
                IntPtr tempIntPrt;
                int gcIntructor = 0;
                try
                {
                    while (true)
                    {
                        try
                        {
                            PingWorker.ActualPing();
                            tempInt = PingWorker.GetLastXAverage();
                            //localTextIcon = new Icon(CreateTextIcon(tempInt.ToString(), bad.Icon), new System.Drawing.Size(32, 32));
                            tempIntPrt = CreateTextIcon(tempInt.ToString(), bad.Icon);

                            using (Icon localTextIcon = new Icon(System.Drawing.Icon.FromHandle(tempIntPrt), new Size(32, 32)))
                            {
                                DestroyIcon(tempIntPrt);
                                if (tempInt < PingWorker.GetGreenRange())
                                {
                                    tempIcon = good.Icon;
                                }
                                else if (tempInt < PingWorker.GetYellowRange())
                                {
                                    tempIcon = okey.Icon;
                                }
                                else
                                {
                                    tempIcon = bad.Icon;

                                }
                                AddIcons(localTextIcon, tempIcon, okey.Icon, notico);
                                if (gcIntructor > 10)
                                {
                                    GC.Collect();
                                    gcIntructor = 0;
                                }
                                gcIntructor++;
                                Thread.Sleep((int)((3600 / PingWorker.GetPingsPerMinute()) * 16.6666f));
                            }
                        }
                        catch (Exception ex)
                        {

                        }

                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

            });
            thread.Start();
            try
            {
                Application.Run();

            }
            catch (Exception ex)
            {

            }
        }

        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        extern static bool DestroyIcon(IntPtr handle);

        public static void AddIcons(Icon originalIcon, Icon overlayIcon, Icon error, NotifyIcon notifyIcon)
        {
            using (Bitmap bitmap = new Bitmap(32, 32, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                try
                {
                    using (Image a = originalIcon.ToBitmap())
                    {
                        using (Image b = overlayIcon.ToBitmap())
                        {
                            //originalIcon.Dispose();
                            //overlayIcon.Dispose();
                            using (Graphics canvas = Graphics.FromImage(bitmap))
                            {
                                canvas.DrawImage(a, 0, 0);
                                canvas.DrawImage(b, 0, 0);
                                canvas.Save();
                                IntPtr hIcon = bitmap.GetHicon();
                                using (Icon temp = System.Drawing.Icon.FromHandle(hIcon))
                                {
                                    notifyIcon.Icon = temp;
                                    DestroyIcon(hIcon);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    notifyIcon.Icon = error;
                }

            }

            //Bitmap bitmap = new Bitmap(32, 32, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

        }

        public static IntPtr CreateTextIcon(string str, Icon error)
        {
            try
            {
                using (Bitmap bitmapText = new Bitmap(32, 32))
                {
                    using (Graphics g = Graphics.FromImage(bitmapText))
                    {
                        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
                        using (Font fontToUse = new Font("Microsoft Sans Serif", 18, FontStyle.Regular, GraphicsUnit.Pixel))
                        {
                            using (Brush brushToUse = new SolidBrush(Color.White))
                            {

                                g.Clear(Color.Transparent);
                                g.DrawString(str, fontToUse, brushToUse, -2, -1);
                                g.Save();
                                IntPtr hIcon = bitmapText.GetHicon();
                                return hIcon;
                                //using (Icon a = new Icon(System.Drawing.Icon.FromHandle(hIcon), new Size(32,32)))
                                //{
                                //    //DestroyIcon(hIcon);
                                //    return a;
                                //}
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //return error;
                return new IntPtr();
            }

        }

        private static void ExitClick(Object sender, EventArgs e)
        {
            OnClosing(sender, e);

            Application.Exit();
        }

        private static void NotifyIconDoubleClick(Object sender, EventArgs e)
        {
            // Was immer du willst

            myForm = Form1.InititalizeForm(PingWorker);
            myForm.textBox2.Text = PingWorker.GetGreenRange().ToString();
            myForm.textBox3.Text = PingWorker.GetYellowRange().ToString();
            myForm.textBox1.Text = PingWorker.GetAdress();
            myForm.numericUpDown1.Value = PingWorker.GetPingsPerMinute();
            Screen[] screens = Screen.AllScreens;
            myForm.Location = new System.Drawing.Point(System.Windows.Forms.Cursor.Position.X - myForm.Width - 34, System.Windows.Forms.Cursor.Position.Y - myForm.Height - 10);
            myForm.Show();
            myForm.BringToFront();
        }

        private static void OnClosing(Object sender, EventArgs e)
        {
            thread.Abort();
            notico.Icon.Dispose();
            notico.Dispose();

        }

        public static void SaveState()
        {
            //FileStream outputStream = new FileStream("ini.txt", FileMode.Create, FileAccess.Write);
            try
            {
                using (FileStream outputStream = File.Create("data\\ini.txt"))
                {
                    AddText(outputStream, PingWorker.GetAdress());
                    AddText(outputStream, PingWorker.GetPingsPerMinute().ToString());
                    AddText(outputStream, PingWorker.GetGreenRange().ToString());
                    AddText(outputStream, PingWorker.GetYellowRange().ToString());
                }
            }
            catch (Exception e)
            {

            }

        }

        private static bool LoadSaveState(Worker worker)
        {
            try
            {
                using (FileStream inputStream = File.OpenRead("data\\ini.txt"))
                {
                    using (StreamReader sr = new StreamReader(inputStream, Encoding.UTF8))
                    {
                        string line = String.Empty;
                        int temp = 0;
                        //      load adress
                        line = sr.ReadLine();
                        if (line != null)
                        {
                            worker.SetAddress(line);
                        }
                        //      load PingPerMinute
                        line = sr.ReadLine();
                        if (line != null)
                        {

                            if (int.TryParse(line, out temp))
                            {
                                worker.SetPingsPerMinute(temp);

                            }
                        }
                        line = sr.ReadLine();
                        string templine = sr.ReadLine();
                        if (line != null && templine != null)
                        {
                            worker.SetRanges(line, templine);
                        }
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                return false;
            }

        }

        private static void AddText(FileStream stream, string input)
        {
            byte[] info = new UTF8Encoding(true).GetBytes(input);
            stream.Write(info, 0, info.Length);
            byte[] newline = Encoding.ASCII.GetBytes(Environment.NewLine);
            stream.Write(newline, 0, newline.Length);
        }

    }
}
