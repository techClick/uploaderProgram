using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using uploader.Properties;

namespace uploader
{
    /// <summary>
    /// Interaction logic for Page1.xaml
    /// </summary>
    public partial class Page2 : Page
    {
        public Page2()
        {
            InitializeComponent();
        }
        public void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(Settings.Default.ID))
            {
                ID.Text = Settings.Default.ID;
            }
            if (!string.IsNullOrWhiteSpace(Settings.Default.Email2))
            {
                email.Text = Settings.Default.Email2;
            }
            if (!string.IsNullOrWhiteSpace(Settings.Default.Files2))
            {
                var files = JArray.Parse(Settings.Default.Files2);
                for (int i = 0; i < files.Count; i++)
                {
                    try
                    {
                        lbFiles.Items.Add(files[i].ToString());
                    }
                    catch
                    {
                        MessageBoxEx.Show(new MainWindow().Wind(), "File " + files[i].ToString() + "\n is broken" +
                            "or moved");
                    }
                }
            }
        }
        private void Text_Changed1(object sender, RoutedEventArgs e)
        {
            Settings.Default.ID = ID.Text;
            Settings.Default.Save();
        }
        private void Text_Changed2(object sender, RoutedEventArgs e)
        {
            Settings.Default.Email2 = email.Text;
            Settings.Default.Save();
        }
        public static string GetDownloadsPath()
        {
            if (Environment.OSVersion.Version.Major < 6) throw new NotSupportedException();
            IntPtr pathPtr = IntPtr.Zero;
            try
            {
                SHGetKnownFolderPath(ref FolderDownloads, 0, IntPtr.Zero, out pathPtr);
                return Marshal.PtrToStringUni(pathPtr);
            }
            finally
            {
                Marshal.FreeCoTaskMem(pathPtr);
            }
        }

        private static Guid FolderDownloads = new Guid("374DE290-123F-4565-9164-39C4925E467B");
        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern int SHGetKnownFolderPath(ref Guid id, int flags, IntPtr token, out IntPtr path);
        public bool IdIsValid(string toCheck)
        {
            if (!new EmailAddressAttribute().IsValid(email.Text))
            {
                MessageBoxEx.Show(new MainWindow().Wind(), "Invalid email address used");
                return false;
            }
            string URI;
            string Parameters;
            if (toCheck == "IDhere" ) {
                URI = "http://jubeeapps.000webhostapp.com/workNew/checkID.php";
                Parameters = "ID=" + Uri.EscapeDataString(ID.Text);
            }
            else
            {
                URI = "http://jubeeapps.000webhostapp.com/workNew/checkEmail.php";
                Parameters = "email=" + Uri.EscapeDataString(email.Text);
            }
            
            using (WebClient wc = new WebClient())
            {
                wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                try
                {
                    string HtmlResult = wc.UploadString(URI, Parameters);
                    if (HtmlResult == "Something")
                    {
                        return true;
                    }
                }
                catch(WebException)
                {

                    MessageBoxEx.Show(new MainWindow().Wind(), "Error: Please check your internet connection!");
                    return false;
                }
            }
            MessageBoxEx.Show(new MainWindow().Wind(), "Email is not recognized on our system");
            return false;
        }
        public void DoubleClickFile(object sender, MouseButtonEventArgs e)
        {
            var item = sender as ListBoxItem;
            if (item != null && item.IsSelected)
            {
                string filePath = GetDownloadsPath() + "\\" + item.Content.ToString();
                try
                {
                    previewBrowser.Navigate(filePath);
                    ///DOES NOT WORK PROPERLY, code to check if file exists;
                }
                catch (FileNotFoundException ex)
                {
                    MessageBoxEx.Show(new MainWindow().Wind(), ex.ToString() );
                }
                return;
            }
            MessageBoxEx.Show(new MainWindow().Wind(), "File not yet downloaded");
        }
        public void DownloadProgressChanged(JArray htmlD, int max, int index , object sender, DownloadProgressChangedEventArgs e )
        {
            double bytesIn = double.Parse(e.BytesReceived.ToString());
            double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
            double percentage = bytesIn / totalBytes * 100;

            progressBar1.Value = int.Parse(Math.Truncate(percentage).ToString());
            int indexHere = index + 1;
            int maxHere = max + 1;
            fileProgress.Text = "Downloading file "+ indexHere + "( of " + maxHere + " )";
            fileName.Text = "This file is: " + htmlD[index];
            progressShow.Text = "PROGRESS: "+ int.Parse(Math.Truncate(percentage).ToString())+"%";
        }
        public void DownloadFileCompleted(JArray html , int max, int index, object sender, AsyncCompletedEventArgs e )
        {
            lbFiles.Items.Add(html[index]);
            Settings.Default.Files2 = JsonConvert.SerializeObject(lbFiles.Items);
            Settings.Default.Save();
            if (index == max)
            {
                fileProgress.Text = "DOWNLOAD COMPLETE";
                int maxHere = max + 1;
                fileName.Text = maxHere + " File(s)";
                progressShow.Text = "DOUBLE CLICK TO VIEW FILES";
                MessageBoxEx.Show(new MainWindow().Wind(), "Download completed");
            }
            else { 
                DownloadThis(html, index + 1);
            }
        }
        public void DownloadThis( JArray HtmlResult , int index )
        {
            string remoteFilename = "http://jubeeapps.000webhostapp.com/workNew/userDocs/" + HtmlResult[index];
            string localFilename = GetDownloadsPath() + "\\" + HtmlResult[index];
            WebClient wc = new WebClient();
            wc.DownloadProgressChanged +=
                new DownloadProgressChangedEventHandler((x, y) =>
                    DownloadProgressChanged(HtmlResult, HtmlResult.Count - 1, index, x, y));
            wc.DownloadFileCompleted +=
                new AsyncCompletedEventHandler((x, y) =>
                    DownloadFileCompleted(HtmlResult,HtmlResult.Count - 1, index, x, y));
            wc.DownloadFileAsync(new Uri(remoteFilename), localFilename);
        }
        public void LoadFiles(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ID.Text) || string.IsNullOrWhiteSpace(email.Text))
            {
                MessageBoxEx.Show(new MainWindow().Wind(), "Error\nPlease fill in all details");
                return;
            }
            if (!IdIsValid("Email"))
            {
                return;
            }
            if (!IdIsValid("IDhere"))
            {
                MessageBoxEx.Show(new MainWindow().Wind(), "Invalid ID, check your mail for your identification number");
                return;
            }
            WebClient wc = new WebClient();
            wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
            string URI = "http://jubeeapps.000webhostapp.com/workNew/getDocs.php";
            string Parameters = "ID=" + Uri.EscapeDataString(ID.Text);
            try
            {
                string HtmlResultTmp = wc.UploadString(URI, Parameters);
                var HtmlResult = JArray.Parse(HtmlResultTmp);
                if (HtmlResult.Count == 0)
                {
                    MessageBoxEx.Show(new MainWindow().Wind(), "Done");
                    fileProgress.Text = "NO FILES TO SHOW";
                }
                else
                {
                    MessageBoxEx.Show(new MainWindow().Wind(), "Details accepted. Download will now begin");
                    fileProgress.Text = "Downloading file 1( of " + HtmlResult.Count + " )";
                    fileName.Text = "This file is: " + HtmlResult[0];
                    fileName.Visibility = Visibility.Visible;
                    progressShow.Text = "PROGRESS: 0%";
                    progressShow.Visibility = Visibility.Visible;
                    progressBar1.Visibility = Visibility.Visible;
                    DownloadThis(HtmlResult, 0);
                }
                uploadFileB.Visibility = Visibility.Collapsed;
            }
            catch (WebException)
            {
                MessageBoxEx.Show(new MainWindow().Wind(), "Error: Please check your internet connection!");
            }
        }

    }
}
