using System;
using System.Data;
using System.Data.OleDb;
using System.IO;

namespace uploader
{
    class MSACCESSload
    {

        OleDbCommand cmd = new OleDbCommand();
        OleDbConnection cn = new OleDbConnection();
        public void saveData(string name, string email, object docs, string resultNum)
        {
            string filePath = Path.GetDirectoryName(System.AppDomain.CurrentDomain.BaseDirectory);
            filePath = Directory.GetParent(Directory.GetParent(filePath).FullName).FullName;
            cn.ConnectionString = @"Provider = Microsoft.ACE.OLEDB.12.0; Data Source = "+filePath+"\\workCH.accdb";
            cmd.Connection = cn;
            try
            {

                cmd.CommandType = CommandType.Text;
                cmd.CommandText = @"INSERT INTO work_style(NameDB, Email, Docs, idnumber) 
                    VALUES('" + name + "', '" + email + "', '" + docs + "', '" + resultNum + "')";
                cmd.Parameters.AddWithValue("@NameDB", name);
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@Docs", docs);
                cmd.Parameters.AddWithValue("@idnumber", resultNum);
                cn.Open();
                cmd.ExecuteNonQuery();
                //System.Windows.Forms.MessageBox.Show("An Item has been successfully added", "Caption", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                cn.Close();
            }
            catch (Exception e)
            {
                cn.Close();
                System.Windows.MessageBox.Show("MICROSOFT ACCESS ERROR: " + e.Message.ToString());
            }
        }
    }
}
