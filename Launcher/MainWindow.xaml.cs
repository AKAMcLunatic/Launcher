using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Net.NetworkInformation;
using MahApps.Metro.Controls;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Drawing;
using System.Windows.Navigation;
using System.Net;
using MahApps.Metro;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using Microsoft.Win32;
using ApplicationUpdate;
using AutoHotkey.Interop;
using System.Threading;
using RedCell.Diagnostics.Update;
using System.Text;
using MahApps.Metro.Controls.Dialogs;

namespace Radar_Starter
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        TextBoxOutputter outputter;
        private NotifyIcon MyNotifyIcon;
        public static string LocalIpAdress;
        public static string AllIpAdress;
        public static string SomeText2;
        public static string SomeText3;
        ViewModel viewModel = new ViewModel();
        public static string TextVer;
        public static string PakURL;
        List<string> PathToJar = new List<string>();
        List<string> PathToMap = new List<string>();
        List<string> PathToMapNames = new List<string>();
        List<string> ColorTheme = new List<string>();
        List<string> ThemeLigDark = new List<string>();
        string PathToPak1 = Path.GetTempPath() + "/TslGame-WindowsNoEditor_ui1.pak";
        string ConnectFilter;

        public MainWindow()
        {
            try
            {
                InitializeComponent();
                LauncherCheckVer2();
                InitProcessJSRadar();
                TextBoxCmd.Text += "------------------------------- Launcher Made by Lafko from https://lafkomods.ru/ -------------------------------";
                ChangeAppStyle();
                ChangeAppTheme();
                DataContext = viewModel;
                TextBoxRadarPCIP.Text = Launcher_Namespace.Properties.Settings.Default.TextBoxRadarPCIP;
                TextBoxGamePCIP.Text = Launcher_Namespace.Properties.Settings.Default.TextBoxGamePCIP;
                TextBoxPak.Text = Launcher_Namespace.Properties.Settings.Default.TextBoxPakPath;
                TextBoxAhk.Text = Launcher_Namespace.Properties.Settings.Default.TextBoxAhkPath;
                SplitTheme.SelectedItem = Launcher_Namespace.Properties.Settings.Default.Color;
                SplitDark.SelectedItem = Launcher_Namespace.Properties.Settings.Default.Theme;
                SplitConnect.SelectedIndex = Launcher_Namespace.Properties.Settings.Default.ComboBoxFilter;
                FindJavaVersion();
                FindWinPcap();
                MyNotifyIcon = new NotifyIcon();
                MyNotifyIcon.Icon = new Icon(Launcher_Namespace.Properties.Resources.Launcher_icon, 40, 40);
                MyNotifyIcon.MouseDoubleClick += new MouseEventHandler(MyNotifyIcon_MouseDoubleClick);
                RadarCheck();
                GetAllFilters();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Launcher error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void LauncherCheckVer2()
        {
            try
            {
                var updater = new Updater();
                updater.StartMonitoring();
                // Log activity to the console.
                Log.Console = true;

                // Log activity to the System.Diagnostics.Debug facilty.
                Log.Debug = true;

                // Prefix messages to the above.
                Log.Prefix = "[Update] "; // This is the default.

                outputter = new TextBoxOutputter(TextBoxCmd);

                // Send activity messages to the UI.
                Log.Event += (sender, e) => outputter.Write("\n" + e.Message);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Update error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        public class TextBoxOutputter : TextWriter
        {
            System.Windows.Controls.TextBox textBox = null;

            public TextBoxOutputter(System.Windows.Controls.TextBox output)
            {
                textBox = output;
            }

            public override void Write(char value)
            {
                base.Write(value);
                textBox.Dispatcher.BeginInvoke(new Action(() =>
                {
                    textBox.AppendText(value.ToString());
                }));
            }

            public override Encoding Encoding
            {
                get { return Encoding.UTF8; }
            }
        }

        private void RadarCheck()
        {
            var v = Directory.GetFiles(Environment.CurrentDirectory, "*.jar", SearchOption.AllDirectories);
            if (v.Length == 0)
            {
                TextBoxCmd.Text += "\nRadar File nod found.";
            }
            foreach (var s in v)
            {
                if (File.Exists(s))
                {
                    TextBoxCmd.Text += "\nRadar file fund in: " + s;
                    ButtonDorU.IsEnabled = false;
                    ButtonDorU.Content = "UPDATE RADAR";
                }
            }
            GetAllMapFiles();
            GetAllJarFiles();
            ComboBoxRadar.SelectedIndex = Launcher_Namespace.Properties.Settings.Default.ComboBoxIndex;
            ComboBoxMap.SelectedIndex = Launcher_Namespace.Properties.Settings.Default.ComboBoxMap;
            string localVersion = Versions.LocalVersion(Environment.CurrentDirectory + "/radar.version");
            string remoteVersion = Versions.RemoteVersion("https://lafkomods.ru/update/radar/" + "radar.version");
            RlocVer.Content = localVersion;
            RLastVer.Content = remoteVersion;
            if (localVersion != remoteVersion)
            {
                ButtonDorU.IsEnabled = true;
            }
        }

        public class ViewModel : INotifyPropertyChanged
        {
            private string _badgeValue;
            public string BadgeValue
            {
                get { return _badgeValue; }
                set { _badgeValue = value; NotifyPropertyChanged(); }
            }

            private string _badgeValue2;
            public string BadgeValue2
            {
                get { return _badgeValue2; }
                set { _badgeValue2 = value; NotifyPropertyChanged(); }
            }

            private string _badgeValue3;
            public string BadgeValue3
            {
                get { return _badgeValue3; }
                set { _badgeValue3 = value; NotifyPropertyChanged(); }
            }

            private string _badgeValue4;
            public string BadgeValue4
            {
                get { return _badgeValue4; }
                set { _badgeValue4 = value; NotifyPropertyChanged(); }
            }

            public event PropertyChangedEventHandler PropertyChanged;
            private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
            {
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void FindJavaVersion()
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = "java.exe";
                psi.Arguments = " -version";
                psi.CreateNoWindow = true;
                psi.RedirectStandardError = true;
                psi.UseShellExecute = false;
                Process pr = Process.Start(psi);
                string strOutput = pr.StandardError.ReadLine().Split(' ')[2].Replace("\"", "");
                TextBoxCmd.Text += "\nJava found, version: " + strOutput;
            }
            catch (Exception ex)
            {
                MessageBoxResult result = System.Windows.MessageBox.Show("\nJava error: " + ex.Message + ", Download?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    Process.Start("http://www.oracle.com/technetwork/java/javase/downloads/jdk8-downloads-2133151.html");
                }
            }
        }

        private void FindWinPcap()
        {
            if (IsProgramInstalled("WinPcap 4.1.3", false) == true)
            {
                TextBoxCmd.Text += "\nWinPcap 4.1.3 found.";
            }
            else
            {
                MessageBoxResult result = System.Windows.MessageBox.Show("\nWinPcap 4.1.3 not found, Download?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (WebClient wc = new WebClient())
                        {
                            wc.DownloadFileCompleted += wc_DownloadFileCompleted;
                            wc.DownloadFileAsync(new Uri("https://www.winpcap.org/install/bin/WinPcap_4_1_3.exe"), Environment.CurrentDirectory + "/WinPcap_4_1_3.exe");
                        }
                    }
                    catch (Exception ex)
                    {
                        TextBoxCmd.Text += "Download Error " + ex.Message;
                    }
                }
            }
        }

        private void wc_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            try
            {
                Process process = new Process();
                process.StartInfo.FileName = Environment.CurrentDirectory + "/WinPcap_4_1_3.exe";
                process.StartInfo.Arguments = "/quiet";
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.Start();
                process.WaitForExit();
                FindWinPcap();
                if (File.Exists(Environment.CurrentDirectory + "/WinPcap_4_1_3.exe"))
                {
                    File.Delete(Environment.CurrentDirectory + "/WinPcap_4_1_3.exe");
                }
            }
            catch (Exception ex)
            {
                TextBoxCmd.Text += "\nWinPcap install error: " + ex.Message;
            }
        }

        void proc_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                TextBoxCmd.Text = TextBoxCmd.Text + "\n" + e.Data;
                TextBoxCmd.ScrollToEnd();
            }));
        }

        public static String file_exe = Environment.GetFolderPath(Environment.SpecialFolder.InternetCache) + "\\app.version";

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ComboBoxRadar.SelectedIndex == -1)
                {
                    System.Windows.MessageBox.Show("Select Radar!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
               if (ComboBoxRadar.SelectedItem.ToString() == "JSRadar")
                {
                    Process proc = new Process();
                    proc.StartInfo.CreateNoWindow = true;
                    proc.StartInfo.UseShellExecute = false;
                    proc.StartInfo.RedirectStandardOutput = true;
                    proc.StartInfo.RedirectStandardError = true;
                    proc.OutputDataReceived += proc_OutputDataReceived;
                    proc.ErrorDataReceived += proc_OutputDataReceived;

                    SomeText3 = TextBoxRadarPCIP.Text;
                    Launcher_Namespace.Properties.Settings.Default.TextBoxRadarPCIP = TextBoxRadarPCIP.Text;
                    Launcher_Namespace.Properties.Settings.Default.TextBoxGamePCIP = TextBoxGamePCIP.Text;
                    Launcher_Namespace.Properties.Settings.Default.Save();

                    proc.StartInfo.FileName = "cmd.exe";
                    proc.StartInfo.WorkingDirectory = PathToJar[ComboBoxRadar.SelectedIndex];
                    proc.StartInfo.Arguments = "/c nodejsx64.exe index.js " + SomeText3;
                    proc.Start();
                    proc.BeginOutputReadLine();
                    proc.BeginErrorReadLine();
                    TextBoxCmd.Text += "\n" + PathToJar[ComboBoxRadar.SelectedIndex] + " nodejsx64.exe index.js " + SomeText3;
                    ButtonStopRadar.Visibility = Visibility.Visible;
                    return;
                }
                var path = Regex.Replace(PathToJar[ComboBoxRadar.SelectedIndex], "\"", "");

                using (Ionic.Zip.ZipFile zip = Ionic.Zip.ZipFile.Read(path))
                {
                    KillJSRadar();
                    zip.UpdateFile(PathToMap[ComboBoxMap.SelectedIndex] + @"\" + PathToMapNames[ComboBoxMap.SelectedIndex] + @"\Erangel_Minimap.png", @"\maps");
                    zip.UpdateFile(PathToMap[ComboBoxMap.SelectedIndex] + @"\" + PathToMapNames[ComboBoxMap.SelectedIndex] + @"\Miramar_Minimap.png", @"\maps");
                    zip.UpdateFile(PathToMap[ComboBoxMap.SelectedIndex] + @"\" + PathToMapNames[ComboBoxMap.SelectedIndex] + @"\Savage_Minimap.png", @"\maps");
                    zip.Save();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Inject map Error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            try
            {
                if (ComboBoxMap.SelectedIndex == -1)
                {
                    System.Windows.MessageBox.Show("Select map!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                Process proc = new Process();
                proc.StartInfo.CreateNoWindow = true;
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardError = true;
                proc.OutputDataReceived += proc_OutputDataReceived;
                proc.ErrorDataReceived += proc_OutputDataReceived;
                if (RadioCustomIp.IsChecked == true)
                {
                    SomeText2 = TextBoxGamePCIP.Text;
                    SomeText3 = TextBoxRadarPCIP.Text;
                    Launcher_Namespace.Properties.Settings.Default.TextBoxRadarPCIP = TextBoxRadarPCIP.Text;
                    Launcher_Namespace.Properties.Settings.Default.TextBoxGamePCIP = TextBoxGamePCIP.Text;
                    Launcher_Namespace.Properties.Settings.Default.Save();

                    proc.StartInfo.FileName = "java";
                    proc.StartInfo.Arguments = " -jar " + PathToJar[ComboBoxRadar.SelectedIndex] + " " + SomeText3 + ConnectFilter + SomeText2;
                    proc.Start();
                    proc.BeginOutputReadLine();
                    proc.BeginErrorReadLine();
                    TextBoxCmd.Text += "\njava -jar " + PathToJar[ComboBoxRadar.SelectedIndex] + " " + SomeText3 + ConnectFilter + SomeText2;
                }
                if (RadioPCAP.IsChecked == true)
                {
                    string LocalIpAdressFilter = LocalIpAdress.Substring(0, LocalIpAdress.LastIndexOf(':'));
                    string AllIpAdressFilter = AllIpAdress.Substring(0, AllIpAdress.LastIndexOf(':'));
                    proc.StartInfo.FileName = "java";
                    proc.StartInfo.Arguments = "-jar " + PathToJar[ComboBoxRadar.SelectedIndex] + " " + LocalIpAdressFilter + ConnectFilter + AllIpAdressFilter + " Offline";
                    proc.Start();
                    proc.BeginOutputReadLine();
                    proc.BeginErrorReadLine();
                    TextBoxCmd.Text += "\njava -jar " + PathToJar[ComboBoxRadar.SelectedIndex] + " " + LocalIpAdressFilter + ConnectFilter + AllIpAdressFilter + " Offline";
                }
                if (RadioArp.IsChecked == true)
                {
                    SomeText2 = TextBoxGamePCIP.Text;
                    SomeText3 = TextBoxRadarPCIP.Text;
                    Launcher_Namespace.Properties.Settings.Default.TextBoxRadarPCIP = TextBoxRadarPCIP.Text;
                    Launcher_Namespace.Properties.Settings.Default.TextBoxGamePCIP = TextBoxGamePCIP.Text;
                    Launcher_Namespace.Properties.Settings.Default.Save();
                    TextBoxCmd.Text += "\n" + SomeText3;
                    Process.Start("arpspoof.exe", SomeText2);
                    Thread.Sleep(2000);
                    proc.StartInfo.FileName = "java";
                    proc.StartInfo.Arguments = "-jar " + PathToJar[ComboBoxRadar.SelectedIndex] + " " + SomeText3 + ConnectFilter + SomeText2;
                    proc.Start();
                    proc.BeginOutputReadLine();
                    proc.BeginErrorReadLine();
                    TextBoxCmd.Text += "\njava -jar " + PathToJar[ComboBoxRadar.SelectedIndex] + " " + SomeText3 + ConnectFilter + SomeText2;
                }
                if (RadioAuto.IsChecked == true)
                {
                    string LocalIpAdressFilter = LocalIpAdress.Substring(0, LocalIpAdress.LastIndexOf(':'));
                    string AllIpAdressFilter = AllIpAdress.Substring(0, AllIpAdress.LastIndexOf(':'));
                    proc.StartInfo.FileName = "java";
                    proc.StartInfo.Arguments = "-jar " + PathToJar[ComboBoxRadar.SelectedIndex] + " " + LocalIpAdressFilter + ConnectFilter + AllIpAdressFilter;
                    proc.Start();
                    proc.BeginOutputReadLine();
                    proc.BeginErrorReadLine();
                    TextBoxCmd.Text += "\njava -jar " + PathToJar[ComboBoxRadar.SelectedIndex] + " " + LocalIpAdressFilter + ConnectFilter + AllIpAdressFilter;
                }
            }
            catch (Exception ex)
            {
                TextBoxCmd.Text += "\nError: " + ex.Message;
            }
        }

        private void GetLocalIp()
        {
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {
                    foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            comboBoxLanInternet.Items.Add(ip.Address.ToString() + ": " + ni.Name);
                            viewModel.BadgeValue = Convert.ToString(comboBoxLanInternet.Items.Count);
                        }
                    }
                }
            }
        }

        private void GetAllLocalIp()
        {
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                {
                    if (ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                    {
                        if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            comboBoxLanInternet2.Items.Add(ip.Address.ToString() + ": " + ni.Name);
                            viewModel.BadgeValue2 = Convert.ToString(comboBoxLanInternet2.Items.Count);
                        }
                    }
                }
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            LocalIpAdress = (string)comboBoxLanInternet.SelectedItem;
        }

        private void ComboBox_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            AllIpAdress = (string)comboBoxLanInternet2.SelectedItem;
        }

        private void TextBox1_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBoxCmd.IsReadOnly = true;
        }

        void MyNotifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.WindowState = WindowState.Normal;
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void RadioAuto_Checked(object sender, RoutedEventArgs e)
        {
            comboBoxLanInternet.Items.Clear();
            comboBoxLanInternet2.Items.Clear();
            GetLocalIp();
            GetAllLocalIp();
            comboBoxLanInternet.SelectedIndex = 0;
            comboBoxLanInternet2.SelectedIndex = 0;
            comboBoxLanInternet.Visibility = Visibility.Visible;
            comboBoxLanInternet2.Visibility = Visibility.Visible;
            Badge1.Visibility = Visibility.Visible;
            Badge2.Visibility = Visibility.Visible;
        }

        private void RadioAuto_Unchecked(object sender, RoutedEventArgs e)
        {
            comboBoxLanInternet.Visibility = Visibility.Hidden;
            comboBoxLanInternet2.Visibility = Visibility.Hidden;
            Badge1.Visibility = Visibility.Hidden;
            Badge2.Visibility = Visibility.Hidden;
        }

        private void RadioPCAP_Checked(object sender, RoutedEventArgs e)
        {
            comboBoxLanInternet.Items.Clear();
            comboBoxLanInternet2.Items.Clear();
            GetLocalIp();
            GetAllLocalIp();
            comboBoxLanInternet.SelectedIndex = 0;
            comboBoxLanInternet2.SelectedIndex = 0;
            comboBoxLanInternet.Visibility = Visibility.Visible;
            comboBoxLanInternet2.Visibility = Visibility.Visible;
            Badge1.Visibility = Visibility.Visible;
            Badge2.Visibility = Visibility.Visible;
        }

        private void RadioPCAP_Unchecked(object sender, RoutedEventArgs e)
        {
            comboBoxLanInternet.Visibility = Visibility.Hidden;
            comboBoxLanInternet2.Visibility = Visibility.Hidden;
            Badge1.Visibility = Visibility.Hidden;
            Badge2.Visibility = Visibility.Hidden;
        }

        private void RadioArp_Checked(object sender, RoutedEventArgs e)
        {
            TextBoxGamePCIP.Visibility = Visibility.Visible;
            TextBoxRadarPCIP.Visibility = Visibility.Visible;
            Badge1.Visibility = Visibility.Hidden;
            Badge2.Visibility = Visibility.Hidden;
        }

        private void RadioArp_Unchecked(object sender, RoutedEventArgs e)
        {
            TextBoxGamePCIP.Visibility = Visibility.Hidden;
            TextBoxRadarPCIP.Visibility = Visibility.Hidden;
            Badge1.Visibility = Visibility.Visible;
            Badge2.Visibility = Visibility.Visible;
        }

        private void RadioCustomIp_Checked(object sender, RoutedEventArgs e)
        {
            TextBoxGamePCIP.Visibility = Visibility.Visible;
            TextBoxRadarPCIP.Visibility = Visibility.Visible;
            Badge1.Visibility = Visibility.Hidden;
            Badge2.Visibility = Visibility.Hidden;
        }

        private void RadioCustomIp_Unchecked(object sender, RoutedEventArgs e)
        {
            TextBoxGamePCIP.Visibility = Visibility.Hidden;
            TextBoxRadarPCIP.Visibility = Visibility.Hidden;
            Badge1.Visibility = Visibility.Visible;
            Badge2.Visibility = Visibility.Visible;
        }

        public void ChangeAppStyle()
        {
            ColorTheme.Add("Red");
            ColorTheme.Add("Green");
            ColorTheme.Add("Blue");
            ColorTheme.Add("Purple");
            ColorTheme.Add("Orange");
            ColorTheme.Add("Lime");
            ColorTheme.Add("Emerald");
            ColorTheme.Add("Teal");
            ColorTheme.Add("Cyan");
            ColorTheme.Add("Cobalt");
            ColorTheme.Add("Indigo");
            ColorTheme.Add("Violet");
            ColorTheme.Add("Pink");
            ColorTheme.Add("Magenta");
            ColorTheme.Add("Crimson");
            ColorTheme.Add("Amber");
            ColorTheme.Add("Yellow");
            ColorTheme.Add("Brown");
            ColorTheme.Add("Olive");
            ColorTheme.Add("Steel");
            ColorTheme.Add("Mauve");
            ColorTheme.Add("Taupe");
            ColorTheme.Add("Sienna");
            foreach (string theme in ColorTheme)
            {
                SplitTheme.Items.Add(theme);
            }
        }

        public void ChangeAppTheme()
        {
            ThemeLigDark.Add("BaseLight");
            ThemeLigDark.Add("BaseDark");
            foreach (string theme2 in ThemeLigDark)
            {
                SplitDark.Items.Add(theme2);
            }
        }

        private void MetroWindow_Closing(object sender, CancelEventArgs e)
        {
            Launcher_Namespace.Properties.Settings.Default.TextBoxPakPath = TextBoxPak.Text;
            Launcher_Namespace.Properties.Settings.Default.TextBoxAhkPath = TextBoxAhk.Text;
            Launcher_Namespace.Properties.Settings.Default.TextBoxRadarPCIP = TextBoxRadarPCIP.Text;
            Launcher_Namespace.Properties.Settings.Default.TextBoxGamePCIP = TextBoxGamePCIP.Text;
            Launcher_Namespace.Properties.Settings.Default.ComboBoxIndex = ComboBoxRadar.SelectedIndex;
            Launcher_Namespace.Properties.Settings.Default.ComboBoxMap = ComboBoxMap.SelectedIndex;
            Launcher_Namespace.Properties.Settings.Default.ComboBoxFilter = SplitConnect.SelectedIndex;
            Launcher_Namespace.Properties.Settings.Default.Save();
            Process[] processesByName = Process.GetProcessesByName("nodejsx64");
            for (int i = 0; i < processesByName.Length; i++)
            {
                MessageBoxResult result = System.Windows.MessageBox.Show("Stop JSRadar?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    processesByName[i].Kill();
                }  
            }
        }

        public void KillJSRadar()
        {
            try
            {
                foreach (Process proc in Process.GetProcessesByName("nodejsx64"))
                {
                    proc.Kill();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }

        public void InitProcessJSRadar()
        {
            Process[] processesByName = Process.GetProcessesByName("nodejsx64");
            for (int i = 0; i < processesByName.Length; i++)
            {
                ButtonStopRadar.Visibility = Visibility.Visible;
            }
        }

        private void MetroWindow_StateChanged_1(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                MyNotifyIcon.BalloonTipTitle = "Minimize Sucessful";
                MyNotifyIcon.BalloonTipText = "Minimized the app Launcher";
                this.ShowInTaskbar = false;
                MyNotifyIcon.Visible = true;
            }
            else if (this.WindowState == WindowState.Normal)
            {
                MyNotifyIcon.Visible = false;
                this.ShowInTaskbar = true;
            }
        }

        public static bool IsProgramInstalled(string displayName, bool x86Platform)
        {
            string uninstallKey = string.Empty;

            if (x86Platform)
            {
                uninstallKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
            }

            else
            {
                uninstallKey = @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall";
            }

            using (RegistryKey rk = Registry.LocalMachine.OpenSubKey(uninstallKey))
            {

                foreach (string skName in rk.GetSubKeyNames())
                {
                    using (RegistryKey sk = rk.OpenSubKey(skName))
                    {
                        if (sk.GetValue("DisplayName") != null && sk.GetValue("DisplayName").ToString().ToUpper().Equals(displayName.ToUpper()))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public static bool Contains(string inputString, string strToSearch)
        {
            return Regex.IsMatch(inputString, strToSearch);
        }

        private void TextBoxJson_Loaded(object sender, RoutedEventArgs e)
        {
            if (File.Exists(Environment.CurrentDirectory + "/settings.json"))
            {
                string PathToFile = Environment.CurrentDirectory + "/settings.json";
                string text = File.ReadAllText(PathToFile);
                TextBoxJson.Text = text;
            }
        }

        private void ButtonSaveJson_Click(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(Environment.CurrentDirectory + "/settings.json"))
            {
                using (FileStream fs = File.Create(Environment.CurrentDirectory + "/settings.json"))
                {
                    Byte[] info = Launcher_Namespace.Properties.Resources.settings;
                    fs.Write(info, 0, info.Length);
                }
                TextBoxJson.Text = File.ReadAllText(Environment.CurrentDirectory + "/settings.json");
            }
            else
            {
                File.WriteAllText(Environment.CurrentDirectory + "/settings.json", TextBoxJson.Text);
            }
        }

        private void BeginDownload(string remoteURL, string downloadToPath, string version, string executeTarget)
        {
            string filePath = Versions.CreateTargetLocation(downloadToPath, version);

            Uri remoteURI = new Uri(remoteURL);
            WebClient downloader = new WebClient();
            downloader.DownloadProgressChanged += new DownloadProgressChangedEventHandler(wb_DownloadProgressChanged);
            downloader.DownloadFileCompleted += new AsyncCompletedEventHandler(downloader_DownloadFileCompleted);
            downloader.DownloadFileCompleted += wc_DownloadFileCompleted2;
            downloader.DownloadFileAsync(remoteURI, filePath + ".zip", new string[] { version, downloadToPath, executeTarget });
        }

        void downloader_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            string[] us = (string[])e.UserState;
            string currentVersion = us[0];
            string downloadToPath = us[1];
            string executeTarget = us[2];

            if (!downloadToPath.EndsWith("\\"))

                downloadToPath += "\\";

            // Download folder + zip file
            string zipName = downloadToPath + currentVersion + ".zip";
            // Download folder\version\ + executable
            string exePath = downloadToPath + currentVersion + "\\" + executeTarget;

            if (new FileInfo(zipName).Exists)
            {
                using (Ionic.Zip.ZipFile zip = new Ionic.Zip.ZipFile(zipName))
                {
                    if (TextVer == "app.version")
                    {
                        if (File.Exists(Environment.CurrentDirectory + "/Launcher.bak"))
                        {
                            File.Delete(Environment.CurrentDirectory + "/Launcher.bak");
                        }
                        File.Move(AppDomain.CurrentDomain.FriendlyName, "Launcher.bak");
                        Thread.Sleep(5000);
                        zip.ExtractAll(downloadToPath, Ionic.Zip.ExtractExistingFileAction.OverwriteSilently);

                        MessageBoxResult result = System.Windows.MessageBox.Show("Launcher updated, restart app!", "Congratulations", MessageBoxButton.OK, MessageBoxImage.Question);
                        if (result == MessageBoxResult.OK)
                        {
                            Process proc = Process.Start(downloadToPath + "\\" + executeTarget);
                            Environment.Exit(0);
                        }
                    }
                    else
                    {
                        zip.ExtractAll(downloadToPath + currentVersion, Ionic.Zip.ExtractExistingFileAction.OverwriteSilently);
                    }
                }
                if (new FileInfo(exePath).Exists)
                {
                    Versions.CreateLocalVersionFile(downloadToPath, TextVer, currentVersion);
                    Process proc = Process.Start(exePath);
                }
                else
                {
                    System.Windows.MessageBox.Show("Problem with download. File does not exist.");
                }
            }
            else
            {
                System.Windows.MessageBox.Show("Problem with download. File does not exist.");
            }
        }

        void wb_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
        }

        private void wc_DownloadFileCompleted2(object sender, AsyncCompletedEventArgs e)
        {
            if (progressBar1.Value == 100)
            {
                ButtonDorU.IsEnabled = false;
                progressBar1.Value = 0;
                ButtonDorU.Content = "Update Done";
                RadarCheck();
                Button1.IsEnabled = true;
            }
        }

        private void ProgBarDownload_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ButtonDorU.IsEnabled = false;
            ButtonDorU.Content = progressBar1.Value + "%...";
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                using (WebClient wc = new WebClient())
                {
                    wc.DownloadFileCompleted += wc_DownloadFileCompleted3;
                    wc.DownloadFileAsync(new Uri(PakURL), PathToPak1);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            ButtonInject.Visibility = Visibility.Hidden;
            ButtonPakClear.Visibility = Visibility.Visible;
        }

        private void wc_DownloadFileCompleted3(object sender, AsyncCompletedEventArgs e)
        {
            try
            {
                File.Move(TextBoxPak.Text + "/TslGame/Content/Paks/TslGame-WindowsNoEditor_erangel_lod.pak", Path.GetTempPath() + "/TslGame-WindowsNoEditor_erangel_lod_orig.pak");
                Thread.Sleep(100);
                File.Move(TextBoxPak.Text + "/TslGame/Content/Paks/TslGame-WindowsNoEditor_ui.pak", TextBoxPak.Text + "/TslGame/Content/Paks/TslGame-WindowsNoEditor_erangel_lod.pak");
                Thread.Sleep(100);
                File.Copy(PathToPak1, TextBoxPak.Text + "/TslGame/Content/Paks/TslGame-WindowsNoEditor_ui.pak");
                System.Windows.MessageBox.Show("Pak injected, now you can run PUBG", "Congratulations", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TextBoxPath_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            DialogResult result = dialog.ShowDialog();
            TextBoxPak.Text = dialog.SelectedPath;
        }

        private void TextBoxAhk_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".ahk";
            dlg.Filter = "ahk Files (*.ahk)|*.ahk|txt Files (*.txt)|*.txt";
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                string filename = dlg.FileName;
                TextBoxAhk.Text = filename;
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            try
            {
                var ahk = AutoHotkeyEngine.Instance;
                if (File.Exists(TextBoxAhk.Text))
                {
                    ahk.LoadFile(TextBoxAhk.Text);
                }
                else
                {
                    System.Windows.MessageBox.Show("Select ahk file", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            Process[] processesByName = Process.GetProcessesByName("TslGame");
            for (int i = 0; i < processesByName.Length; i++)
            {
                processesByName[i].Kill();
            }
            processesByName = Process.GetProcessesByName("BattlEye Launcher");
            for (int i = 0; i < processesByName.Length; i++)
            {
                processesByName[i].Kill();
            }
            processesByName = Process.GetProcessesByName("BEService.exe");
            for (int i = 0; i < processesByName.Length; i++)
            {
                processesByName[i].Kill();
            }
            processesByName = Process.GetProcessesByName("BroCrashReporter");
            for (int i = 0; i < processesByName.Length; i++)
            {
                processesByName[i].Kill();
            }
            try
            {
                File.Delete(TextBoxPak.Text + "/TslGame/Content/Paks/TslGame-WindowsNoEditor_ui.pak");
                File.Move(TextBoxPak.Text + "/TslGame/Content/Paks/TslGame-WindowsNoEditor_erangel_lod.pak", TextBoxPak.Text + "/TslGame/Content/Paks/TslGame-WindowsNoEditor_ui.pak");
                Thread.Sleep(100);
                File.Move(Path.GetTempPath() + "/TslGame-WindowsNoEditor_erangel_lod_orig.pak", TextBoxPak.Text + "/TslGame/Content/Paks/TslGame-WindowsNoEditor_erangel_lod.pak");
                System.Windows.MessageBox.Show("Inject deleted, Just in case, check the game files.", "Congratulations", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            ButtonPakClear.Visibility = Visibility.Hidden;
            ButtonInject.Visibility = Visibility.Visible;
        }

        private void RadiButtonPak1_Checked(object sender, RoutedEventArgs e)
        {
            PakURL = "http://j25940kk.beget.tech/pubg/pak/TslGame-WindowsNoEditor_ui1.pak";
        }

        private void RadioButtonPak2_Checked(object sender, RoutedEventArgs e)
        {
            PakURL = "http://j25940kk.beget.tech/pubg/pak/TslGame-WindowsNoEditor_ui2.pak";
        }

        private void RadioButtonPak3_Checked(object sender, RoutedEventArgs e)
        {
            PakURL = "http://j25940kk.beget.tech/pubg/pak/TslGame-WindowsNoEditor_ui3.pak";
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            PakURL = "http://j25940kk.beget.tech/pubg/pak/TslGame-WindowsNoEditor_ui4.pak";
        }

        private void RadioButton_Checked_1(object sender, RoutedEventArgs e)
        {
            PakURL = "http://j25940kk.beget.tech/pubg/pak/TslGame-WindowsNoEditor_ui5.pak";
        }

        private void GetAllJarFiles()
        {
            var v = Directory.GetFiles(Environment.CurrentDirectory, "*.jar", SearchOption.AllDirectories);
            foreach (var b in v)
            {
                PathToJar.Add("\"" + Path.GetDirectoryName(b) + "\\" + Path.GetFileName(b) + "\"");
                ComboBoxRadar.Items.Add(Path.GetFileName(b));
                viewModel.BadgeValue3 = Convert.ToString(ComboBoxRadar.Items.Count);
            }
            var va = Directory.GetFiles(Environment.CurrentDirectory, "nodejsx64.exe", SearchOption.AllDirectories);
            foreach (var b in va)
            {
                if (Path.GetFileName(b) == "nodejsx64.exe")
                {
                    PathToJar.Add(Path.GetDirectoryName(b));
                    ComboBoxRadar.Items.Add("JSRadar");
                }
            }
            viewModel.BadgeValue3 = Convert.ToString(ComboBoxRadar.Items.Count);
        }

        private void GetAllMapFiles()
        {
            var v = Directory.GetDirectories(Environment.CurrentDirectory, "Radar Map *", SearchOption.AllDirectories);
            foreach (var b in v)
            {
                PathToMap.Add(Path.GetDirectoryName(b));
                PathToMapNames.Add(Path.GetFileName(b));
                ComboBoxMap.Items.Add(Path.GetFileName(b));
            }
            viewModel.BadgeValue4 = Convert.ToString(ComboBoxMap.Items.Count);
        }

        private void GetAllFilters()
        {
            SplitConnect.Items.Add("PortFilter");
            SplitConnect.Items.Add("PPTPFilter");
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            settingsFlyout.IsOpen = true;
        }

        private void LaunchAppsOnGitHub(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/Lafko/Launcher/");
        }

        private async void ShowProgressDialog(object sender, RoutedEventArgs e)
        {
            var mySettings = new MetroDialogSettings()
            {
                NegativeButtonText = "Close now",
                AnimateShow = false,
                AnimateHide = false
            };

            TextVer = "radar.version";
            string downloadToPath = Environment.CurrentDirectory;
            string localVersion = Versions.LocalVersion(downloadToPath + "/radar.version");
            string remoteURL = "https://lafkomods.ru/update/radar/";
            string remoteVersion = Versions.RemoteVersion(remoteURL + "radar.version");
            string remoteFile = remoteURL + remoteVersion + ".zip";
            if (localVersion != remoteVersion)
            {
                Button1.IsEnabled = false;
                string filePath = Versions.CreateTargetLocation(downloadToPath, remoteVersion);
                var controller = await this.ShowProgressAsync("Please wait...", "We are finding some radar!", settings: mySettings);
                controller.SetIndeterminate();
                await System.Threading.Tasks.Task.Delay(2000);
                Uri remoteURI = new Uri(remoteFile);
                WebClient downloader = new WebClient();
                downloader.DownloadProgressChanged += new DownloadProgressChangedEventHandler(wb_DownloadProgressChanged);
                downloader.DownloadFileCompleted += new AsyncCompletedEventHandler(downloader_DownloadFileCompleted);
                downloader.DownloadFileCompleted += wc_DownloadFileCompleted2;
                downloader.DownloadFileAsync(remoteURI, filePath + ".zip", new string[] { remoteVersion, downloadToPath, "update.txt" });
                controller.SetCancelable(false);
                double i = 1.0;
                while (i < 99.9)
                {
                    double val = progressBar1.Value;
                    controller.SetProgress(val / 100);
                    controller.SetMessage("Downloading radar: " + val + "%...");

                    i = val;

                    await System.Threading.Tasks.Task.Delay(500);
                }
                await controller.CloseAsync();
                if (controller.IsCanceled)
                {
                    await this.ShowMessageAsync("No radar!", "You stopped downloading radar!");
                }
                else
                {
                    MessageDialogResult result = await this.ShowMessageAsync("Downloading Radar finished!", "Clear the directory from zip files?", MessageDialogStyle.AffirmativeAndNegative);
                    if (result == MessageDialogResult.Affirmative)
                    {
                        var v = Directory.GetFiles(Environment.CurrentDirectory, "*.zip", SearchOption.AllDirectories);
                        foreach (var s in v)
                        {
                            File.Delete(s);
                        }
                    }
                }
            }
        }

        private void Themes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ThemeManager.ChangeAppStyle(this, ThemeManager.GetAccent(ColorTheme[SplitTheme.SelectedIndex]), ThemeManager.GetAppTheme(Launcher_Namespace.Properties.Settings.Default.Theme));
            Launcher_Namespace.Properties.Settings.Default.Color = ColorTheme[SplitTheme.SelectedIndex];
        }

        private void Themes_SelectionChanged2(object sender, SelectionChangedEventArgs e)
        {
            ThemeManager.ChangeAppStyle(this, ThemeManager.GetAccent(Launcher_Namespace.Properties.Settings.Default.Color), ThemeManager.GetAppTheme(ThemeLigDark[SplitDark.SelectedIndex]));
            Launcher_Namespace.Properties.Settings.Default.Theme = ThemeLigDark[SplitDark.SelectedIndex];
        }

        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            TextBoxCmd.Text = "";
        }

        private void Connect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SplitConnect.SelectedIndex == 0)
            {
                ConnectFilter = " PortFilter ";
            }
            else
            {
                ConnectFilter = " PPTPFilter ";
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            KillJSRadar();
            ButtonStopRadar.Visibility = Visibility.Hidden;
        }

        private void ComboBoxRadar_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBoxRadar.SelectedItem.ToString() == "JSRadar")
            {
                TextBoxGamePCIP.IsEnabled = false;
                RadioAuto.IsEnabled = false;
                RadioPCAP.IsEnabled = false;
                RadioArp.IsEnabled = false;
                RadioAuto.IsChecked = false;
                RadioPCAP.IsChecked = false;
                RadioArp.IsChecked = false;
                RadioCustomIp.IsChecked = true;
                ComboBoxMap.IsEnabled = false;
                ComboBoxMap.SelectedIndex = -1;
                lablRadarPCIP.Content = "ENTER LOCAL IP ADDRESS";
            }
            else
            {
                TextBoxGamePCIP.IsEnabled = true;
                RadioAuto.IsEnabled = true;
                RadioPCAP.IsEnabled = true;
                RadioArp.IsEnabled = true;
                ComboBoxMap.IsEnabled = true;
                lablRadarPCIP.Content = "RADAR PC IP";
            }
        }
    }
}