using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Net;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows;
using System.Windows.Controls;
using Path = System.IO.Path;
using System.IO;
using System.Diagnostics;
using uploader.Properties;
using Newtonsoft.Json.Linq;

namespace uploader
{
    /// <summary>
    /// Interaction logic for Page1.xaml
    /// </summary>
    public partial class Page1 : Page
    {
        public Page1()
        {
            InitializeComponent();
        }
        public void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(Settings.Default.Name))
            {
                userName.Text = Settings.Default.Name;
            }
            if (!string.IsNullOrWhiteSpace(Settings.Default.Email))
            {
                email.Text = Settings.Default.Email;
            }
            if (!string.IsNullOrWhiteSpace(Settings.Default.Files1))
            {
                var files = JArray.Parse(Settings.Default.Files1);
                for (int i=0; i<files.Count; i++)
                {
                    try
                    {
                        lbFiles.Items.Add(Path.GetFileName(files[i].ToString()));
                        lbFilesPath.Items.Add(Path.GetFullPath(files[i].ToString()));
                        lbFilesName.Items.Add(files[i].ToString());
                        clear.Visibility = Visibility.Visible;
                    }
                    catch(FileNotFoundException)
                    {
                        MessageBoxEx.Show(new MainWindow().Wind(), "File " + files[i].ToString() + "\n is broken" +
                            "or moved" );
                    }
                }
            }
        }
        private void Text_Changed1(object sender, RoutedEventArgs e)
        {
            Settings.Default.Name = userName.Text;
            Settings.Default.Save();
        }
        private void Text_Changed2(object sender, RoutedEventArgs e)
        {
            Settings.Default.Email = email.Text;
            Settings.Default.Save();
        }
        private void ClearFiles(object sender, RoutedEventArgs e)
        {
            for (int i=lbFiles.Items.Count-1;i>=0;i--)
            {
                lbFiles.Items.RemoveAt(i);
                lbFilesPath.Items.RemoveAt(i);
                lbFilesName.Items.RemoveAt(i);
            }
            clear.Visibility = Visibility.Collapsed;
            Settings.Default.Files1 = JsonConvert.SerializeObject(lbFilesName.Items);
            Settings.Default.Save();
        }
        public void SaveData()
        {
            string resultNum = "";
            string characters = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            Random rnd = new Random();
            for (int i = 0; i < 9; i++)
            {
                resultNum = resultNum + characters[rnd.Next(62)];
            }
            string URI = "http://jubeeapps.000webhostapp.com/workNew/savelist.php";
            string Parameters = "email=" + Uri.EscapeDataString(email.Text) + "&name=" + Uri.EscapeDataString(userName.Text) +
                "&docs=" + Uri.EscapeDataString(JsonConvert.SerializeObject(lbFiles.Items))+"&numID="+
                resultNum;
            using (WebClient wc = new WebClient())
            {
                try
                {
                    wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                    string HtmlResult = wc.UploadString(URI, Parameters);
                }
                catch (WebException)
                {
                    fileName.Text = lbFiles.Items.Count + " File(s)";
                    progressShow.Visibility = Visibility.Collapsed;
                    MessageBoxEx.Show(new MainWindow().Wind(), "Error: Please check your internet connection!");
                    return;
                }
            }
            new MSSERVERload().saveData(userName.Text, email.Text, JsonConvert.SerializeObject(lbFiles.Items), resultNum);
            new MSACCESSload().saveData(userName.Text, email.Text, JsonConvert.SerializeObject(lbFiles.Items), resultNum);
            info1.Text = "UPLOAD COMPLETE";
            fileName.Text = lbFiles.Items.Count + " File(s)";
            progressShow.Visibility = Visibility.Collapsed;
            MessageBoxEx.Show( new MainWindow().Wind() , "Your files/profile is saved, check your email for your identification number and confirmation");
        }
        public void UploadProgressChanged(int index, object sender, UploadProgressChangedEventArgs e)
        {
            //WebBrowser.Navigate("C:\\word.doc");
            double bytesIn = double.Parse(e.BytesReceived.ToString());
            double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
            double percentage = ( bytesIn / totalBytes ) * 100;

            progressBar1.Value = int.Parse(Math.Truncate(percentage).ToString());
            int indexHere = index + 1;
            info1.Text = "Uploading file " + indexHere + "( of " + lbFiles.Items.Count + " )";
            fileName.Text = "This file is: " + lbFiles.Items[index];
            progressShow.Text = "PROGRESS: " + int.Parse(Math.Truncate(percentage).ToString()) + "%";
        }
        public void UploadFileCompleted( int index, object sender, AsyncCompletedEventArgs e)
        {
            if (index == lbFiles.Items.Count - 1)
            {
                SaveData();
            }
            else
            {
                downloadThis(index + 1);
            }
        }
        public void downloadThis( int index)
        {
            string remoteFilename = "http://jubeeapps.000webhostapp.com/workNew/userDocs/" + lbFiles.Items[index];
            WebClient wc = new WebClient();
            wc.UploadProgressChanged +=
                new UploadProgressChangedEventHandler((x, y) =>
                    UploadProgressChanged( index, x, y));
            wc.UploadFileCompleted +=
                new UploadFileCompletedEventHandler((x, y) =>
                    UploadFileCompleted(index, x, y));
            try
            {
                wc.UploadFileAsync(new Uri("http://jubeeapps.000webhostapp.com/workNew/saveFile.php"),
                    lbFilesPath.Items[index].ToString());
            }
            catch (WebException)
            {
                MessageBoxEx.Show(new MainWindow().Wind(), "Error: Please check your internet connection!");
            }
        }
        public void SaveFiles(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(userName.Text))
            {
                MessageBoxEx.Show(new MainWindow().Wind(), "Please fill in your name");
                return;
            }
            if (string.IsNullOrWhiteSpace(email.Text) )
            {
                MessageBoxEx.Show(new MainWindow().Wind(), "Please fill in your name email address");
                return;
            }
            if ( !new EmailAddressAttribute().IsValid(email.Text) )
            {
                MessageBoxEx.Show(new MainWindow().Wind(), "Invalid email address used");
                return;
            }
            if (lbFiles.Items.Count == 0)
            {
                info1.Text = "No files";
                SaveData();
            }
            else
            {
                info1.Text = "Uploading file 1( of " + lbFiles.Items.Count + " )";
                fileName.Text = "This file is: " + lbFiles.Items[0];
                fileName.Visibility = Visibility.Visible;
                progressShow.Text = "PROGRESS: 0%";
                progressShow.Visibility = Visibility.Visible;
                progressBar1.Visibility = Visibility.Visible;
                downloadThis(0);
            }
        }
        private void UploadFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Multiselect = true;
            dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (dlg.ShowDialog() == true)
            {
                foreach (string filename in dlg.FileNames)
                {
                    FileInfo fi = new FileInfo(filename);
                    var size = fi.Length;
                    Trace.Write(size);
                    if ( size > 50000000 )
                    {
                        MessageBoxEx.Show(new MainWindow().Wind(), "File size ERROR: all files must be under 5MB");
                        return;
                    }
                }
                foreach (string filename in dlg.FileNames)
                {

                    lbFiles.Items.Add(Path.GetFileName(filename));
                    lbFilesPath.Items.Add(Path.GetFullPath(filename));
                    lbFilesName.Items.Add(filename);
                    clear.Visibility = Visibility.Visible;
                }
                Settings.Default.Files1 = JsonConvert.SerializeObject(lbFilesName.Items);
                Settings.Default.Save();
            }
        }
    }
}
