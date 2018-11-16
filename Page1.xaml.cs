using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Path = System.IO.Path;

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
        public void SaveData()
        {
            string resultNum = "";
            string characters = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            Random rnd = new Random();
            for (int i = 0; i < 9; i++)
            {
                resultNum = resultNum + characters[rnd.Next(62)];
            }
            new MSSERVERload().saveData(userName.Text, email.Text, JsonConvert.SerializeObject(lbFiles.Items) , resultNum );
            new MSACCESSload().saveData(userName.Text, email.Text, JsonConvert.SerializeObject(lbFiles.Items), resultNum );
            string URI = "http://jubeeapps.000webhostapp.com/workNew/savelist.php";
            string Parameters = "email=" + Uri.EscapeDataString(email.Text) + "&name=" + Uri.EscapeDataString(userName.Text) +
                "&docs=" + Uri.EscapeDataString(JsonConvert.SerializeObject(lbFiles.Items))+"&numID="+
                resultNum;
            using (WebClient wc = new WebClient())
            {
                wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                string HtmlResult = wc.UploadString(URI, Parameters);
            }
            info1.Text = "UPLOAD COMPLETE";
            fileName.Text = lbFiles.Items.Count + " File(s)";
            progressShow.Visibility = Visibility.Collapsed;
            MessageBox.Show("Your Files/Profile is saved, check your email for your Identification number and confirmation");
        }
        public void UploadProgressChanged(int index, object sender, UploadProgressChangedEventArgs e)
        {
            //WebBrowser.Navigate("C:\\word.doc");
            double bytesIn = double.Parse(e.BytesReceived.ToString());
            double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
            double percentage = bytesIn / totalBytes * 100;

            progressBar1.Value = int.Parse(Math.Truncate(percentage).ToString());
            int indexHere = index + 1;
            info1.Text = "Uploading File " + indexHere + "( of " + lbFiles.Items.Count + " )";
            fileName.Text = "This File is: " + lbFiles.Items[index];
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
            //wc.DownloadFile( remoteFilename , localFilename);
            WebClient wc = new WebClient();
            wc.UploadProgressChanged +=
                //new DownloadProgressChangedEventHandler(DownloadProgressChanged);
                new UploadProgressChangedEventHandler((x, y) =>
                    UploadProgressChanged( index, x, y));
            wc.UploadFileCompleted +=
                new UploadFileCompletedEventHandler((x, y) =>
                    UploadFileCompleted(index, x, y));

            // Starts the download
            wc.UploadFileAsync( new Uri( "http://jubeeapps.000webhostapp.com/workNew/saveFile.php" ) , 
                lbFilesPath.Items[index].ToString());
            //wc.DownloadFile(remoteFilename, localFilename);
        }
        public bool IsValidEmail(string source)
        {
            return new EmailAddressAttribute().IsValid(source);
        }
        public void SaveFiles(object sender, RoutedEventArgs e)
        {
            info1.Text = "Sending File";

            if (string.IsNullOrWhiteSpace(userName.Text) || string.IsNullOrWhiteSpace(email.Text) ||
                !IsValidEmail(email.Text))
            {
                if (!string.IsNullOrWhiteSpace(email.Text) && !IsValidEmail(email.Text))
                {
                    MessageBox.Show("Invalid email address used");
                }
                else
                {
                    MessageBox.Show("Not enough data to save profile");
                }
                return;
            }
            if (lbFiles.Items.Count == 0)
            {
                SaveData();
            }
            else
            {
                info1.Text = "Uploading File 1( of " + lbFiles.Items.Count + " )";
                fileName.Text = "This File is: " + lbFiles.Items[0];
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
            //dlg.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (dlg.ShowDialog() == true)
            {
                foreach (string filename in dlg.FileNames)
                {
                    lbFiles.Items.Add(Path.GetFileName(filename));
                    lbFilesPath.Items.Add(Path.GetFullPath(filename));
                }
            }
        }
    }
}
